using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Repositories;

public interface IProductRepository
{
    Task<Result<List<Domain.Domains.Catalog.Entities.Product>>> GetProductListByIdAsync(List<Guid> productIds,bool forUpdate = false);
    
    
    Task<Result<PagedList<Domain.Domains.Catalog.Entities.Product>>> GetProductListAsync(PaginationParams paginationParams);

    void UpdateRangeAsync(List<Domain.Domains.Catalog.Entities.Product> products);
}