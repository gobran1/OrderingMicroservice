namespace SharedKernel.Entity;

public interface IHasDomainEvent
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    public void ClearDomainEvents();
    
    public void AddDomainEvent(IDomainEvent @event);
}