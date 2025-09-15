using AutoMapper;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
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
        
        return Result<GetOrderDetailsDTO>.Success(orderDto);
    }
}
