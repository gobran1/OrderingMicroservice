using AutoMapper;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using Order.Domain.Domains.Order.Entities;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Platform.Observability;

namespace Order.Application.Features.Order.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<GetOrderDetailsDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductGrpcClient _productGrpcClient;
    private readonly ILogger _logger;
    private static readonly ActivitySource ActivitySource = new("Order.Application", "1.0.0");

    public CreateOrderCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IProductGrpcClient productGrpcClient,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _productGrpcClient = productGrpcClient;
        _logger = logger;
    }
    
    public async Task<Result<GetOrderDetailsDTO>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(Tracing.ActivityNames.OrderCreation);
        var stopwatch = Stopwatch.StartNew();
        var userId = request.CreateOrderDto.User?.UserId.ToString() ?? "guest";
        
        activity?.SetTag(Tracing.Tags.ProductCount, request.CreateOrderDto.OrderItems.Count);
        activity?.SetTag(Tracing.Tags.ProductIds, string.Join(",", request.CreateOrderDto.OrderItems.Select(x => x.ProductId)));
        
        _logger.LogInformation(
            "Starting order creation for user {UserId} with {ProductCount} items with ids {productIds}",
            userId, 
            request.CreateOrderDto.OrderItems.Count,
            string.Join(",", request.CreateOrderDto.OrderItems.Select(x => x.ProductId)));

        Result<List<GetProductDTO>> productListResult = Result<List<GetProductDTO>>.Success(new List<GetProductDTO>());
        
        try
        {
            var productIdList = request.CreateOrderDto.OrderItems.Select(x => x.ProductId).ToList();
            
            using (var productsDetailsActivity = ActivitySource.StartActivity(Tracing.ActivityNames.GetProductsDetails))
            {
                productsDetailsActivity.AddExternalServiceContext("ProductService", "GetProductsByIdAsync");
                productsDetailsActivity?.SetTag(Tracing.Tags.ProductIds, string.Join(",", productIdList));
                
                _logger.LogInformation("Get list of products with ids: {productIds}", String.Join(",",productIdList));
                
                 productListResult = await _productGrpcClient.GetProductsByIdAsync(productIdList);
                
                // Record external service metrics
                BusinessMetrics.ExternalServiceCalls.Add(1,
                    new KeyValuePair<string, object?>(BusinessMetrics.Labels.ExternalService, "ProductService"),
                    new KeyValuePair<string, object?>(BusinessMetrics.Labels.Operation, "GetProductsDetail")
                    );
                
                if (productListResult.Value == null || productListResult.IsFailure)
                {
                    var errorMsg = productListResult.Error ?? "Product validation failed";
                    productsDetailsActivity.RecordBusinessError("GetProductsDetail", errorMsg);
                    
                    BusinessMetrics.ExternalServiceErrors.Add(1, 
                        new KeyValuePair<string, object?>(BusinessMetrics.Labels.ExternalService, "ProductService"),
                        new KeyValuePair<string, object?>(BusinessMetrics.Labels.Operation, "GetProductsDetail")
                        );
                    
                    _logger.LogWarning("Get Product Detail Grpc call failed,error: {Error}", errorMsg);
                    productsDetailsActivity.RecordBusinessError("ExternalServiceError", errorMsg);
                    
                    return Result<GetOrderDetailsDTO>.Failure(errorMsg);
                }
                
                productsDetailsActivity.RecordSuccess("Products validated successfully");
                
                _logger.LogInformation("Successfully validated {ProductCount} products", productIdList.Count);
            }
            
            // Step 2: Build order items
            var orderItemList = new List<OrderItem>();
            var productListDic = productListResult.Value.ToDictionary(x=>x.ProductId);
            
            foreach (var i in request.CreateOrderDto.OrderItems)
            {
                if (!productListDic.TryGetValue(i.ProductId, out var product))
                {
                    var errorMsg = $"Product {i.ProductId} not found";
                    
                    activity.RecordBusinessError("ProductNotFound", errorMsg);
                    BusinessMetrics.BusinessErrors.Add(1, new KeyValuePair<string, object?>(BusinessMetrics.Labels.ErrorType, "ProductNotFound"));
                    _logger.LogError("Product {ProductId} not found", i.ProductId);
                    
                    return Result<GetOrderDetailsDTO>.Failure(errorMsg);
                }
                
                orderItemList.Add(new OrderItem(
                    Guid.Empty,
                    new Quantity(i.Quantity.Amount, i.Quantity.Uom),
                    new ProductVO(
                        productId: product.ProductId,
                        productName: product.ProductName,
                        productSku: product.ProductSku
                    ),
                    price: new Money(product.ProductPrice.Amount, Enum.Parse<CurrencyCode>(product.ProductPrice.Currency))
                ));
            }
            
            // Step 3: Create order domain entity
            var order = Domain.Domains.Order.Entities.Order.Create(
                _mapper.Map<Address>(request.CreateOrderDto.DeliveryAddress),
                _mapper.Map<UserVO>(request.CreateOrderDto.User),
                orderItemList
            );
            
            using (var dbActivity = ActivitySource.StartActivity(Tracing.ActivityNames.DatabaseOperation))
            {
                dbActivity.AddDatabaseContext("Orders", "CreateOrder");
                dbActivity?.SetTag(Tracing.Tags.OrderId, order.Id.ToString());
                
                _logger.LogInformation("Persisting order {OrderId} to database", order.Id);
                
                var storeOrderResult = await _unitOfWork.OrderRepository.CreateOrderAsync(order);
                await _unitOfWork.SaveChangesAsync();
                
                BusinessMetrics.DatabaseOperations.Add(1, new KeyValuePair<string, object?>(BusinessMetrics.Labels.DatabaseTable, "Orders"));
                
                if (storeOrderResult.IsFailure)
                {
                    var errorMsg = storeOrderResult.Error ?? "Failed to store order";
                   
                    dbActivity.RecordBusinessError("DatabaseError", errorMsg);
                    BusinessMetrics.BusinessErrors.Add(1, new KeyValuePair<string, object?>(BusinessMetrics.Labels.ErrorType, "DatabaseError"));
                    _logger.LogError("Failed to store order {OrderId}: {Error}", order.Id, errorMsg);
                    
                    return Result<GetOrderDetailsDTO>.Failure(errorMsg);
                }
                
                dbActivity.RecordSuccess("Order stored successfully");
                _logger.LogInformation("Successfully persisted order {OrderId} to database", order.Id);
            }
            
            // Step 5: Record success metrics and logs
            stopwatch.Stop();
            var processingTimeSeconds = stopwatch.Elapsed.TotalSeconds;

            var totalAmount = order.Total;
            BusinessMetrics.OrdersCreated.Add(1, 
                new KeyValuePair<string, object?>(BusinessMetrics.Labels.Currency, totalAmount.Currency),
                new KeyValuePair<string, object?>(BusinessMetrics.Labels.UserId, userId));
            BusinessMetrics.OrderValue.Record(totalAmount.Amount, new KeyValuePair<string, object?>(BusinessMetrics.Labels.Currency, totalAmount.Currency));
            BusinessMetrics.OrderProcessingTime.Record(processingTimeSeconds);
            
            activity.AddBusinessContext(userId,order.Id.ToString(),totalAmount.Amount,totalAmount.Currency.ToString());
            
            activity.RecordSuccess("Order created successfully");
            activity?.SetTag("order.processing_time_seconds", processingTimeSeconds);
            
            _logger.LogInformation(
                "Successfully created order {OrderId} for user {UserId} with total amount {TotalAmount} {Currency} in {ProcessingTime:F2}s",
                order.Id, userId, totalAmount.Amount, totalAmount.Currency, processingTimeSeconds);
            
            return Result<GetOrderDetailsDTO>.Success(_mapper.Map<GetOrderDetailsDTO>(order));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity.RecordBusinessError("UnexpectedError", ex.Message);
            BusinessMetrics.BusinessErrors.Add(1, new KeyValuePair<string, object?>(BusinessMetrics.Labels.ErrorType, "UnexpectedError"));
            _logger.LogError(ex, "Unexpected error occurred while creating order for user {UserId}", userId);
            
            throw;
        }
    }
}