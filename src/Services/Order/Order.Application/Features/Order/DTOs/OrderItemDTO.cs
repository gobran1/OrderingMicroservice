using Contract.SharedDTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Features.Order.DTOs;

public class OrderItemDTO
{
}

public class GetOrderItemDTO : OrderItemDTO
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    
    public GetProductVODTO Product { get; set; }

    public GetQuantityDTO Quantity { get; set; }
   
    public GetMoneyDTO Price { get; set; }
}

public class CreateOrderItemDTO : OrderItemDTO
{
    public CreateQuantityDTO Quantity { get; set; }
    public Guid ProductId { get; set; }
}