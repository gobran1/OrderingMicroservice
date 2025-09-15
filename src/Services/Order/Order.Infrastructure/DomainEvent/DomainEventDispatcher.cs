using MediatR;
using Order.Application.Common;
using SharedKernel.Entity;
using SharedKernel.Messaging;
using static System.Activator;

namespace Order.Infrastructure.DomainEvent;

public class DomainEventDispatcher:IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());

            if (CreateInstance(notificationType, domainEvent) is INotification domainEventNotification) 
                await _mediator.Publish(domainEventNotification);
        }
    }
}