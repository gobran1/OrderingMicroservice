using AutoMapper;
using MediatR;
using Order.Application.Common;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using Order.Application.Features.Order.Repositories;
using Order.Domain.Domains.Order.Entities;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Features.Order.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<GetOrderDetailsDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductGrpcClient _productGrpcClient;

    public CreateOrderCommandHandler(IMapper mapper,IUnitOfWork unitOfWork,IProductGrpcClient productGrpcClient)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _productGrpcClient = productGrpcClient;
    }
    
    public async Task<Result<GetOrderDetailsDTO>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIdList = request.CreateOrderDto.OrderItems.Select(x => x.ProductId).ToList();
        
        var productListResult = await _productGrpcClient.GetProductsByIdAsync(productIdList);
        
        if (productListResult.Value == null || productListResult.IsFailure)
        {
            return Result<GetOrderDetailsDTO>.Failure(productListResult.Error ?? string.Empty);
        }
        
        var orderItemList = new List<OrderItem>();
        var productListDic = productListResult.Value.ToDictionary(x=>x.ProductId);
        var error = "";
        foreach (var i in request.CreateOrderDto.OrderItems)
        {
            if (!productListDic.TryGetValue(i.ProductId,out var product))
            {
                error = "Product not found";
                break;
            }
            
            orderItemList.Add(new OrderItem(
                Guid.Empty
                ,new Quantity(i.Quantity.Amount, i.Quantity.Uom),
                new ProductVO
                (
                    productId: product.ProductId,
                    productName: product.ProductName,
                    productSku: product.ProductSku
                ),
                price: new Money(product.ProductPrice.Amount,Enum.Parse<CurrencyCode>(product.ProductPrice.Currency))
            ));
        }

        if (!string.IsNullOrEmpty(error))
        {
            return Result<GetOrderDetailsDTO>.Failure(error);
        }

        var order = Domain.Domains.Order.Entities.Order.Create(
            _mapper.Map<Address>(request.CreateOrderDto.DeliveryAddress),
            _mapper.Map<UserVO>(request.CreateOrderDto.User),
            orderItemList
        );

        var storeOrderResult = await _unitOfWork.OrderRepository.CreateOrderAsync(order);
       
        await _unitOfWork.SaveChangesAsync();
        
        if (storeOrderResult.IsFailure)
        {
            return Result<GetOrderDetailsDTO>.Failure(storeOrderResult.Error??string.Empty);
        }
        
        return Result<GetOrderDetailsDTO>.Success(_mapper.Map<GetOrderDetailsDTO>(order));
    }
}