using System.Diagnostics;
using AutoMapper;
using Inventory.Application.Features.Product.DTOs;
using Inventory.Application.Features.Product.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Observability;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public class GetProductsQueryHandler:IRequestHandler<GetProductsQuery,Result<PagedList<GetProductDTO>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsQueryHandler> _logger;
    private readonly ActivitySource _activitySource = new("Inventory.Application", "1.0.0");

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<Result<PagedList<GetProductDTO>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetProductListAsync(request.paginationParams);
        
        if (productResult.IsFailure)
        {
            _logger.LogWarning("Failed to get products. Error={Error}", productResult.Error);
            return Result<PagedList<GetProductDTO>>.Failure(productResult.Error ?? string.Empty); 
        }
        
        var products = _mapper.Map<PagedList<GetProductDTO>>(productResult.Value);
        
        return Result<PagedList<GetProductDTO>>.Success(products);
    }
    
}