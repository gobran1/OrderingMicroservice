using Contract.SharedDTOs;

namespace Contract.Inventory.Dtos;

public class GetProductDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string Sku { get; set; }

    public GetMoneyDTO Price { get; set; }
}