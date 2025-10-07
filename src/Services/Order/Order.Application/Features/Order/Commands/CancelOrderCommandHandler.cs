using AutoMapper;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using Platform.Observability;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Commands;

public class CancelOrderCommandHandler:IRequestHandler<CancelOrderCommand,Result<GetOrderDetailsDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    
    public async Task<Result<GetOrderDetailsDTO>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await _unitOfWork.OrderRepository.GetOrderAsync(request.OrderId,true);
        
        if (orderResult.IsFailure)
            return Result<GetOrderDetailsDTO>.Failure(orderResult.Error ?? String.Empty);
        
        var order = orderResult.Value;
        
        order.Cancel();
        
        await _unitOfWork.SaveChangesAsync();
        
        var orderDto = _mapper.Map<GetOrderDetailsDTO>(order);
        
        BusinessMetrics.OrdersCancelled.Add(1,
            new KeyValuePair<string, object?>(BusinessMetrics.Labels.UserId,order.User.UserId)
            );
        BusinessMetrics.OrderCancelledValue.Record(order.Total.Amount,
            new KeyValuePair<string, object?>(BusinessMetrics.Labels.Currency,order.Total.Currency.ToString()),
            new KeyValuePair<string, object?>(BusinessMetrics.Labels.UserId,order.User.UserId.ToString())
            );
        
        return Result<GetOrderDetailsDTO>.Success(orderDto);
    }
}
