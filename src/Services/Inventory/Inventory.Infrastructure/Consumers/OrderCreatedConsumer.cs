using System.Diagnostics;
using System.Diagnostics.Metrics;
using Contract.Inventory.Events;
using Contract.Order.Events;
using Inventory.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Platform.Observability;
using SharedKernel.Entity;
using SharedKernel.Messaging;

namespace Inventory.Infrastructure.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEventV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly ActivitySource activitySource = new("Inventory.Infrastructure", "1.0.0");
    public OrderCreatedConsumer(
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        ILogger<OrderCreatedConsumer> logger
    )
    {
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<OrderCreatedEventV1> context)
    {
        if (await _unitOfWork.ProcessedMessageRepository.CheckExist(context.Message.Id))
            return;
        
        using var activity = activitySource.StartActivity(Tracing.ActivityNames.OrderFulfillment);
        activity?.SetTag(Tracing.Tags.OrderId, context.Message.OrderId.ToString());
        activity?.SetTag("order.items.count", context.Message.Items.Count);
        
        _logger.LogInformation("Processing order stock reservation. OrderId={OrderId} ItemCount={ItemCount}", 
            context.Message.OrderId, 
            context.Message.Items.Count);
        
        try
        {
            // Check for duplicate processing
            await _unitOfWork.ProcessedMessageRepository.InsertAsync(new ProcessedMessage
            {
                MessageId = context.Message.Id,
                ProcessedAt = DateTime.UtcNow
            });
            
            // Get all products for the order
            var productIds = context.Message.Items.Select(x => x.ProductId).ToList();
            var productsResult = await _unitOfWork.ProductRepository.GetProductListByIdAsync(productIds, true);
            
            if (productsResult.IsFailure || productsResult.Value == null)
            {
                activity?.RecordError("database", "cannot fetch order products");
                _logger.LogError("Failed to fetch products for order. OrderId={OrderId} Error={Error}", 
                    context.Message.OrderId, productsResult.Error);
                throw new ArgumentException("cannot fetch order products");
            }
            
            // Reserve stock for each item
            var productsDic = productsResult.Value.ToDictionary(x => x.Id);
            var failedItem = (string?)null;
            
            foreach (var item in context.Message.Items)
            {
                if (!productsDic.TryGetValue(item.ProductId, out var product))
                {
                    failedItem = $"Product {item.ProductId} not found";
                    break;
                }
                
                var reserveResult = product.ReserveStock(item.Quantity);
                if (reserveResult.IsFailure)
                {
                    failedItem = $"Insufficient stock for product {item.ProductId}";
                    break;
                }
            }
            
            if (failedItem != null)
            {
                activity?.RecordBusinessError("stock_reservation_failed", failedItem);
                _logger.LogWarning("Stock reservation failed. OrderId={OrderId} Reason={Reason}", 
                    context.Message.OrderId, failedItem);
                    
                BusinessMetrics.OrderProductsReservationErrors.Add(1,
                    new KeyValuePair<string, object?>(BusinessMetrics.Labels.ErrorType, "unavailable_stock"));
                
                await _messageBus.PublishAsync(new SalesStockFailedEventV1(context.Message.OrderId));
                return;
            }
            
            // Publish success event and save changes
            await _messageBus.PublishAsync(new SalesStockReservedEventV1(context.Message.OrderId));
            await _unitOfWork.SaveChangesAsync();
            
            BusinessMetrics.OrderProductsReservation.Add(1);
            
            _logger.LogInformation("Stock reserved successfully. OrderId={OrderId}", context.Message.OrderId);

        }
        catch (Exception e)
        {
            activity?.RecordError("exception", e.Message);
            _logger.LogError(e, "Error processing order stock reservation. OrderId={OrderId}", context.Message.OrderId);
            
            BusinessMetrics.OrderProductsReservationErrors.Add(1,
                new KeyValuePair<string, object?>(BusinessMetrics.Labels.ErrorType, "exception"));
                
            throw;
        }

    }
}