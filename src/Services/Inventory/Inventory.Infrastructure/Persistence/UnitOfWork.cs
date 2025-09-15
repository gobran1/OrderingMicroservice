using Inventory.Application.Common.Interfaces;
using Inventory.Application.Features.Product.Repositories;
using SharedKernel.Entity;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    public IProductRepository ProductRepository { get; }
    public IProcessedMessageRepository ProcessedMessageRepository { get; }
    private readonly InventoryDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    
    public UnitOfWork(
        InventoryDbContext inventoryDbContext,
        IDomainEventDispatcher domainEventDispatcher,
        IProductRepository productRepository,
        IProcessedMessageRepository processedMessageRepository
    )
    {
        ProductRepository = productRepository;
        _dbContext = inventoryDbContext;
        _domainEventDispatcher = domainEventDispatcher;
        ProcessedMessageRepository = processedMessageRepository;
    }
    
    
    public async Task SaveChangesAsync()
    {
        await PublishDomainEvent();
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task PublishDomainEvent()
    {
        var domainEvents = _dbContext.ChangeTracker
            .Entries<IHasDomainEvent>()
            .Where(x=> x.Entity.DomainEvents.Count != 0)
            .SelectMany(x=>x.Entity.DomainEvents)
            .ToList();
        
        await _domainEventDispatcher.DispatchAsync(domainEvents);
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}