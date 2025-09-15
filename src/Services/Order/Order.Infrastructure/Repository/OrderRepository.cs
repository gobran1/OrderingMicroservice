using Microsoft.EntityFrameworkCore;
using Order.Application.Features.Order.Repositories;
using Order.Infrastructure.Extensions;
using Order.Infrastructure.Persistence;
using SharedKernel.DTOs;

namespace Order.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly DbSet<Domain.Domains.Order.Entities.Order> _orders;

    public OrderRepository(OrderDbContext dbContext)
    {
        _orders = dbContext.Set<Domain.Domains.Order.Entities.Order>();
    }

    public async Task<Result<Domain.Domains.Order.Entities.Order>> CreateOrderAsync(
        Domain.Domains.Order.Entities.Order order)
    {
        await _orders.AddAsync(order);
        return Result<Domain.Domains.Order.Entities.Order>.Success(order);
    }

    public async Task<Result<Domain.Domains.Order.Entities.Order>> GetOrderAsync(Guid orderId,bool forUpdate = false)
    {
        var orderQuery = _orders.AsQueryable();

        if (!forUpdate)
        {
            orderQuery = orderQuery.AsNoTracking();
        }
        
        var order = await orderQuery.Where(o => o.Id == orderId)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync();
        
        if (order == null)
            return Result<Domain.Domains.Order.Entities.Order>.Failure("Order not found");

        return Result<Domain.Domains.Order.Entities.Order>.Success(order);
    }
    
    public async Task<Result<PagedList<Domain.Domains.Order.Entities.Order>>> GetOrderPaginatedListAsync(PaginationParams paginationParams)
    {
        var orders = await _orders
            .AsNoTracking()
            .OrderByDescending(x=>x.CreatedAt)
            .ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        
        return Result<PagedList<Domain.Domains.Order.Entities.Order>>.Success(orders);
    }
    
    
}