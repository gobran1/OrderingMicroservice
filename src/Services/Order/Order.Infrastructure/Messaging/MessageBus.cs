using MassTransit;
using SharedKernel.Messaging;

namespace Order.Infrastructure.Messaging;

public class MessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publish;

    public MessageBus(IPublishEndpoint publishEndpoint)
    {
        _publish = publishEndpoint;
    }
    
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        await _publish.Publish(@event, cancellationToken);
    }
}