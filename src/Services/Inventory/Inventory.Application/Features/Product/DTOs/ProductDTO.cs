using Contract.SharedDTOs;
using SharedKernel.ValueObjects;

namespace Inventory.Application.Features.Product.DTOs;

public class ProductDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public GetMoneyDTO Price { get; set; }
    public string Sku { get; set; }

    public GetQuantityDTO Quantity { get; set; }

}

public class GetProductDTO:ProductDTO
{
    public Guid Id { get; set; }
    
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}