using AutoMapper;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery,Result<GetOrderDetailsDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<Result<GetOrderDetailsDTO>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderResult= await _unitOfWork.OrderRepository.GetOrderAsync(request.OrderId);

        if (orderResult.IsFailure)
        {
            return Result<GetOrderDetailsDTO>.Failure(orderResult.Error ?? String.Empty);
        }
        
        return Result<GetOrderDetailsDTO>.Success(_mapper.Map<GetOrderDetailsDTO>(orderResult.Value));
    }
}