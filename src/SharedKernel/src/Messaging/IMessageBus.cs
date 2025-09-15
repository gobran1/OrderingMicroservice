namespace SharedKernel.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T @event,CancellationToken cancellationToken = default) where T : IntegrationEvent;
}