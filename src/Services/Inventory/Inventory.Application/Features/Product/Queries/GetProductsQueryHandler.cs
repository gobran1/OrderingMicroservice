using AutoMapper;
using Inventory.Application.Features.Product.DTOs;
using Inventory.Application.Features.Product.Repositories;
using MediatR;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public class GetProductsQueryHandler:IRequestHandler<GetProductsQuery,Result<PagedList<GetProductDTO>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository productRepository,IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
    
    public async Task<Result<PagedList<GetProductDTO>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetProductListAsync(request.paginationParams);
        
        if (productResult.IsFailure)
        {
            return Result<PagedList<GetProductDTO>>.Failure(productResult.Error ?? string.Empty); 
        }
        
        var products = _mapper.Map<PagedList<GetProductDTO>>(productResult.Value);
        
        return Result<PagedList<GetProductDTO>>.Success(products);
    }
    
}