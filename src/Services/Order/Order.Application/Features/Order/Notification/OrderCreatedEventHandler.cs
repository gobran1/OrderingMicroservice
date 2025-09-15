using Contract.Order.Events;
using MediatR;
using Order.Application.Common;
using Order.Domain.Domains.Order.Events;
using SharedKernel.Messaging;

namespace Order.Application.Features.Order.Notification;

public class OrderCreatedEventHandler : INotificationHandler<DomainEventNotification<OrderCreatedEvent>>
{
    private readonly IMessageBus _messageBus;

    public OrderCreatedEventHandler(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
    
    public async Task Handle(DomainEventNotification<OrderCreatedEvent> notification, CancellationToken cancellationToken)
    {
        await _messageBus.PublishAsync(new OrderCreatedEventV1
        {
            OrderId = notification.DomainEntity.Order.Id,
            CreatedAt = DateTime.UtcNow,
            Items = notification.DomainEntity.Order.OrderItems.Select(i=>new OrderCreatedEventV1Items
            {
                Id = i.Id,
                ProductId = i.Product.ProductId,
                Quantity = i.Quantity,
            }).ToList()
        }, cancellationToken);
    }
}