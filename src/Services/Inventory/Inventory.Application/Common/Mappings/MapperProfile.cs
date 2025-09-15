using AutoMapper;
using Contract.SharedDTOs;
using Inventory.Application.Features.Product.DTOs;
using Inventory.Domain.Domains.Catalog.Entities;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;

namespace Inventory.Application.Common.Mappings;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Guid, string>().ConvertUsing(g => g == Guid.Empty ? null : g.ToString());
        CreateMap<string, Guid>().ConvertUsing(s => 
            string.IsNullOrWhiteSpace(s) ? Guid.Empty : Guid.Parse(s));
        
        CreateMap<DateTime,string>().ConvertUsing(d=>d.ToString("yyyy-MM-dd HH:mm:ss"));
        
        CreateMap<Money,GetMoneyDTO>();
        CreateMap<Quantity,GetQuantityDTO>();
        
        CreateMap<Product,GetProductDTO>();
        
        CreateMap<PagedList<Product>, PagedList<GetProductDTO>>()
            .ForMember(d=>d.Items,opt=>opt.MapFrom(s=>s.Items))
            .ForMember(d=>d.TotalItems,opt=>opt.MapFrom(s=>s.TotalItems))
            .ForMember(d=>d.TotalPages,opt=>opt.MapFrom(s=>s.TotalPages))
            ;
    }
}