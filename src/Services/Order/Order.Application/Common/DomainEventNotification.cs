using MediatR;
using SharedKernel.Entity;

namespace Order.Application.Common;

public class DomainEventNotification<TDomainEntity> : INotification where TDomainEntity:IDomainEvent
{
    public TDomainEntity DomainEntity { get; }
    
    public DomainEventNotification(TDomainEntity domainEntity) => DomainEntity = domainEntity;
}