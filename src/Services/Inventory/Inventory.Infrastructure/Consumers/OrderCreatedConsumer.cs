using Contract.Inventory.Events;
using Contract.Order.Events;
using Inventory.Application.Common.Interfaces;
using MassTransit;
using SharedKernel.Entity;
using SharedKernel.Messaging;

namespace Inventory.Infrastructure.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEventV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;

    public OrderCreatedConsumer(IUnitOfWork unitOfWork,IMessageBus messageBus)
    {
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
    }
    
    public async Task Consume(ConsumeContext<OrderCreatedEventV1> context)
    {
        if (await _unitOfWork.ProcessedMessageRepository.CheckExist(context.Message.Id))
            return;
    
        await _unitOfWork.ProcessedMessageRepository.InsertAsync(new ProcessedMessage
        {
            MessageId = context.Message.Id,
            ProcessedAt = DateTime.UtcNow
        });
        
        var orderItems = context.Message.Items;
        
        var productIds = orderItems.Select(x => x.ProductId).ToList();

        var productsResult = await _unitOfWork.ProductRepository.GetProductListByIdAsync(productIds,true);

        if (productsResult.IsFailure || productsResult.Value == null)
            throw new ArgumentException("cannot fetch order products");

        var productsDic = productsResult.Value.ToDictionary(x=>x.Id);

        var invalidStock = false;
        foreach (var item in orderItems)
        {
            if (!productsDic.TryGetValue(item.ProductId, out var product))
            {
                invalidStock = true;
                break;
            }
            
            var reserveResult = product.ReserveStock(item.Quantity);
            
            if (reserveResult.IsFailure)
            {
                invalidStock = true;
                break;
            }
        }
        
        if (invalidStock)
        {
            await _messageBus.PublishAsync(new SalesStockFailedEventV1(context.Message.OrderId));
            return;
        }
        
        await _messageBus.PublishAsync(new SalesStockReservedEventV1(context.Message.OrderId));
        
        await _unitOfWork.SaveChangesAsync();
    }
}