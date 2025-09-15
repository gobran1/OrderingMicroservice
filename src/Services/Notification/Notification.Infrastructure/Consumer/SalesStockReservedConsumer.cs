using Contract.Inventory.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Notification.Infrastructure.Consumer;

public class SalesStockReservedConsumer : IConsumer<SalesStockReservedEventV1>
{
    private readonly ILogger<SalesStockReservedConsumer> _logger;

    public SalesStockReservedConsumer(ILogger<SalesStockReservedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SalesStockReservedEventV1> context)
    {
        var orderId = context.Message.OrderId.ToString();
        
        _logger.LogInformation("Order {OrderId} is processed successfully", orderId);
        
        return Task.CompletedTask;
    }
}