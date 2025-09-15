using Order.Application.Features.Order.Repositories;
using SharedKernel.Repositories;

namespace Order.Application.Common.Interfaces;

public interface IUnitOfWork
{
    public IOrderRepository OrderRepository { get; }
    public IOrderItemRepository OrderItemRepository { get; }
    public IProcessedMessageRepository ProcessedMessageRepository { get; }
    
    public Task SaveChangesAsync();
}