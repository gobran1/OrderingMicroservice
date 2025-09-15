using Inventory.Application.Features.Product.Repositories;
using SharedKernel.Repositories;

namespace Inventory.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    IProcessedMessageRepository ProcessedMessageRepository { get; }
    
    public Task SaveChangesAsync();
}