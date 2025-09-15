using System.Security.AccessControl;
using Contract.SharedDTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Features.Order.DTOs;

public class OrderDTO
{
    
    
}

public class GetOrderListDTO : OrderDTO
{
    public Guid Id { get; set; }
    public GetAddressDTO DeliveryAddress { get; set; }
    
    public string OrderNumber { get; set; }

    public GetMoneyDTO Total { get; set; }
    
    public string Status { get; set; }
    
    public GetUserDTO User { get; set; }

    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}

public class GetOrderDetailsDTO : GetOrderListDTO
{
    public List<GetOrderItemDTO> OrderItems { get; set; }
}

public class CreateOrderDTO : OrderDTO
{
    public List<CreateOrderItemDTO> OrderItems { get; set; }
    public CreateUserVoDTO User { get; set; }
    public CreateAddressDTO? DeliveryAddress { get; set; }
}