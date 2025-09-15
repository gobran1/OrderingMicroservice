using AutoMapper;
using Contract.SharedDTOs;
using Order.Application.Features.Order.DTOs;
using Order.Domain.Domains.Order.Entities;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Common.Mappings;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<DateTime,string>().ConvertUsing(d=>d.ToString("yyyy-MM-dd HH:mm:ss"));

        CreateMap<PagedList<Domain.Domains.Order.Entities.Order>, PagedList<GetOrderListDTO>>();
        CreateMap<Domain.Domains.Order.Entities.Order, GetOrderListDTO>();
        
        CreateMap<Domain.Domains.Order.Entities.Order, GetOrderDetailsDTO>();
        CreateMap<OrderItem, GetOrderItemDTO>();

        CreateMap<Address, GetAddressDTO>();
        CreateMap<CreateAddressDTO,Address>();
        CreateMap<UserVO, GetUserDTO>();
        CreateMap<CreateUserVoDTO, UserVO>();
        CreateMap<Money, GetMoneyDTO>();
        CreateMap<Quantity, GetQuantityDTO>();
        
        
        CreateMap<OrderItem, GetOrderItemDTO>();
        CreateMap<ProductVO, GetProductVODTO>();

    }
}