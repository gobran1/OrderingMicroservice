using Contract.Inventory.Events;
using MassTransit;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.Commands;
using Platform.Observability;
using SharedKernel.Entity;

namespace Order.Infrastructure.Consumer;

public class SalesStockFailedConsumer : IConsumer<SalesStockFailedEventV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public SalesStockFailedConsumer(IUnitOfWork unitOfWork,IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }
    
    public async Task Consume(ConsumeContext<SalesStockFailedEventV1> context)
    {
        if (await _unitOfWork.ProcessedMessageRepository.CheckExist(context.Message.Id))
            return;
        
        await _unitOfWork.ProcessedMessageRepository.InsertAsync(new ProcessedMessage
        {
            MessageId = context.Message.Id,
            ProcessedAt = DateTime.UtcNow
        });
        
        var orderId = context.Message.OrderId;
        
       await  _mediator.Send(new CancelOrderCommand(context.Message.OrderId));

       await _unitOfWork.SaveChangesAsync();
       
       BusinessMetrics.OrdersCancelled.Add(1,
           new KeyValuePair<string, object?>("order_id", orderId.ToString()));
       
    }
}