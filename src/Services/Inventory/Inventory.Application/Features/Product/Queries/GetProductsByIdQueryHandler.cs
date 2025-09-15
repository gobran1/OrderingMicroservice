using AutoMapper;
using Inventory.Application.Features.Product.DTOs;
using Inventory.Application.Features.Product.Repositories;
using MediatR;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public class GetProductsByIdQueryHandler:IRequestHandler<GetProductsByIdQuery,Result<List<GetProductDTO>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsByIdQueryHandler(IProductRepository productRepository,IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
    
    public async Task<Result<List<GetProductDTO>>> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetProductListByIdAsync(request.ProductIds.Select(Guid.Parse).ToList());
        
        if (productResult.IsFailure)
        {
            return Result<List<GetProductDTO>>.Failure(productResult.Error ?? string.Empty); 
        }
        
        var products = _mapper.Map<List<GetProductDTO>>(productResult.Value);
        
        return Result<List<GetProductDTO>>.Success(products);
    }
    
}