using SharedKernel.Entity;

namespace Order.Domain.Domains.Order.Events;

public record OrderCreatedEvent(Entities.Order Order) : IDomainEvent;