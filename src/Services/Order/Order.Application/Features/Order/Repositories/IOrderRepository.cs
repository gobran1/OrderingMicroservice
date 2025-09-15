using SharedKernel.DTOs;


namespace Order.Application.Features.Order.Repositories;

public interface IOrderRepository
{
    Task<Result<Domain.Domains.Order.Entities.Order>> CreateOrderAsync(Domain.Domains.Order.Entities.Order order);
    
    Task<Result<Domain.Domains.Order.Entities.Order>> GetOrderAsync(Guid orderId,bool forUpdate = false);

    Task<Result<PagedList<Domain.Domains.Order.Entities.Order>>> GetOrderPaginatedListAsync(
        PaginationParams paginationParams);
    
}