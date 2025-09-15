using Contract.Inventory.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Notification.Infrastructure.Consumer;

public class SalesStockFailedConsumer : IConsumer<SalesStockFailedEventV1>
{
    private readonly ILogger<SalesStockFailedConsumer> _logger;

    public SalesStockFailedConsumer(ILogger<SalesStockFailedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SalesStockFailedEventV1> context)
    {
        var orderId = context.Message.OrderId.ToString();
        
        _logger.LogWarning("Order {OrderId} is cancelled because of insufficient stock", orderId);
       
        return Task.CompletedTask;
    }
}