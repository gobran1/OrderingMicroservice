using Google.Protobuf;
using Grpc.Core;
using Inventory.Application.Features.Product.Queries;
using MediatR;
using Product.V1;

namespace Inventory.Api.Products.Services;

public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private readonly IMediator _mediator;

    public ProductServiceImpl(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public override async Task<GetProductsResponse> GetProductsById(GetProductsByIdRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetProductsByIdQuery(request.ProductIds.ToList()));

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, result.Error ?? string.Empty));
        }
        
        var response = new GetProductsResponse();

        response.Products.AddRange(
            result?.Value?.Select(p => new GetProductResponse
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price.Amount,
                Currency = p.Price.Currency.ToString(),
                Sku = p.Sku
            }).ToList()
        );

        return response;
    }
}