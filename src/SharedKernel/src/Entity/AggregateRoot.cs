namespace SharedKernel.Entity;

public abstract class AggregateRoot<TId> : Entity<TId> , IHasDomainEvent
{
    private readonly List<IDomainEvent> _events = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent @event)
    {
        _events.Add(@event);
    }
    
    public void ClearDomainEvents()
    {
        _events.Clear();
    }
}