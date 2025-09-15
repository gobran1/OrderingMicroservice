using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entity;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Repository;

public class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly InventoryDbContext _dbContext;
    private readonly DbSet<ProcessedMessage> _processedMessages;

    public ProcessedMessageRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
        _processedMessages = dbContext.Set<ProcessedMessage>();
    }

    public async Task InsertAsync(ProcessedMessage message)
    {
        await _processedMessages.AddAsync(message);
    }
    
    public async Task<bool> CheckExist(Guid messageId)
    {
        return await _processedMessages.AsNoTracking().AnyAsync(x=>x.MessageId == messageId);
    }
}