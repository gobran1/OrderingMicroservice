using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Inventory.Infrastructure.Extensions;

public static class PagesListExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
            long totalItems = await query.LongCountAsync();
            
            List<T> items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, totalItems, pageSize);
    }
    
}