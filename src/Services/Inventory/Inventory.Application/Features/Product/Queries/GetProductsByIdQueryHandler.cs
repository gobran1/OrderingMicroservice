using System.Diagnostics;
using AutoMapper;
using Inventory.Application.Features.Product.DTOs;
using Inventory.Application.Features.Product.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Observability;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public class GetProductsByIdQueryHandler:IRequestHandler<GetProductsByIdQuery,Result<List<GetProductDTO>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsByIdQueryHandler> _logger;
    private readonly ActivitySource _activitySource = new("Inventory.Application", "1.0.0");
    public GetProductsByIdQueryHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<GetProductsByIdQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<Result<List<GetProductDTO>>> Handle(GetProductsByIdQuery request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetProductListByIdAsync(request.ProductIds.Select(Guid.Parse).ToList());
            
        if (productResult.IsFailure)
        {
            _logger.LogWarning("Failed to get products by IDs. ProductIds={ProductIds} Error={Error}", 
                String.Join(",", request.ProductIds), 
                productResult.Error);
        
            return Result<List<GetProductDTO>>.Failure(productResult.Error ?? string.Empty); 
        }
        
        var products = _mapper.Map<List<GetProductDTO>>(productResult.Value);
        
        return Result<List<GetProductDTO>>.Success(products);
    }
    
}