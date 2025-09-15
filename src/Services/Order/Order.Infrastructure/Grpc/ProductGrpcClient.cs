using Contract.SharedDTOs;
using Google.Protobuf;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using Product.V1;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;

namespace Order.Infrastructure.Grpc;

public class ProductGrpcClient : IProductGrpcClient
{
    private readonly ProductService.ProductServiceClient _productServiceClient;

    public ProductGrpcClient(ProductService.ProductServiceClient productServiceClient)
    {
        _productServiceClient = productServiceClient;
    }
    
    public async Task<Result<List<GetProductDTO>>> GetProductsByIdAsync(List<Guid> ids)
    {
        var getProductsByIdRequest = new GetProductsByIdRequest();
        getProductsByIdRequest.ProductIds.AddRange(ids.Select(i=>i.ToString()).ToList());
        
       var getProductsByIdResponse = await _productServiceClient.GetProductsByIdAsync(getProductsByIdRequest);

       var productList =  getProductsByIdResponse.Products.Select(p => new GetProductDTO
       {
           ProductId = Guid.Parse(p.Id),
           ProductName = p.Name,
           ProductPrice = new GetMoneyDTO{Amount = p.Price,Currency = p.Currency},
           ProductSku = p.Sku
       }).ToList();
       
       return Result<List<GetProductDTO>>.Success(productList);
    }
    
}