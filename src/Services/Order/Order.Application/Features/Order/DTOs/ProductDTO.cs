
using Contract.SharedDTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Features.Order.DTOs;

public class ProductDTO
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductSku { get; set; }
}


public class GetProductVODTO : ProductDTO;

public class GetProductDTO : ProductDTO
{
    public GetMoneyDTO ProductPrice { get; set; }
};
