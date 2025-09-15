using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.Repositories;
using Order.Infrastructure.Repository;
using SharedKernel.Entity;
using SharedKernel.Messaging;
using SharedKernel.Repositories;

namespace Order.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly OrderDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    public IOrderRepository OrderRepository { get; }
    public IOrderItemRepository OrderItemRepository { get; }
    
    public IProcessedMessageRepository ProcessedMessageRepository { get; }
    
    
    public UnitOfWork(
        OrderDbContext orderDbContext,
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IProcessedMessageRepository processedMessageRepository
    )
    {
        _dbContext = orderDbContext;
        _domainEventDispatcher = domainEventDispatcher;
        ProcessedMessageRepository = processedMessageRepository;
        OrderRepository = orderRepository;
        OrderItemRepository = orderItemRepository;
    }
    
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
        await PublishDomainEvent();
    }

    public async Task PublishDomainEvent()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<IHasDomainEvent>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToList();
        
        var domainEvents = domainEntities
            .SelectMany(x=>x.Entity.DomainEvents)
            .ToList();
        
        domainEntities.ToList().ForEach(x=>x.Entity.ClearDomainEvents());
        
        await _domainEventDispatcher.DispatchAsync(domainEvents);
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}