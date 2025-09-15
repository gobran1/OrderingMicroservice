using Inventory.Application.Features.Product.Repositories;
using Inventory.Domain.Domains.Catalog.Entities;
using Inventory.Infrastructure.Extensions;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Inventory.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _dbContext;
    private readonly DbSet<Product> _products;

    public ProductRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
        _products = dbContext.Set<Product>();
    }

    public async Task<Result<List<Product>>> GetProductListByIdAsync(List<Guid> productIds,bool forUpdate = false)
    {
        var productQuery = _products.AsQueryable();
        
        if (!forUpdate)
        {
            productQuery = productQuery.AsNoTracking();
        }
        
       var products = await productQuery.Where(x => productIds.Contains(x.Id))
            .ToListAsync();

        return Result<List<Product>>.Success(products);
    }

    public async Task<Result<PagedList<Product>>> GetProductListAsync(PaginationParams paginationParams)
    {
        var products = await _products.AsNoTracking()
            .OrderByDescending(o => o.Id)
            .ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        
        
        return Result<PagedList<Product>>.Success(products);
    }


    public void UpdateRangeAsync(List<Product> products)
    {
        foreach (var product in products)
        { 
            _products.Attach(product);
            _dbContext.Entry(product).State = EntityState.Modified;
        }
    }
    
}