using SharedKernel.DTOs;
using SharedKernel.Entity;
using SharedKernel.ValueObjects;

namespace Inventory.Domain.Domains.Catalog.Entities;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public Money Price { get; set; }

    public string Sku { get; set; }

    public Quantity Quantity { get; set; }


    public Result ReserveStock(Quantity requiredQuantity)
    {
        if (requiredQuantity.Amount > Quantity.Amount)
            return Result.Failure("no enough stock");
        
        Quantity.Amount -= requiredQuantity.Amount;
        
        return Result.Success();
    }
}