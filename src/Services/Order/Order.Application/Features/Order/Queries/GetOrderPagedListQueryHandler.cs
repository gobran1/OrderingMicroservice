using AutoMapper;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Queries;

public class GetOrderPagedListQueryHandler:IRequestHandler<GetOrderPagedListQuery,Result<PagedList<GetOrderListDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderPagedListQueryHandler(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<Result<PagedList<GetOrderListDTO>>> Handle(GetOrderPagedListQuery request, CancellationToken cancellationToken)
    {
        var orderResult = await _unitOfWork.OrderRepository.GetOrderPaginatedListAsync(request.PaginationParams);

        if (orderResult.IsFailure)
        {
            return Result<PagedList<GetOrderListDTO>>.Failure(orderResult.Error ?? string.Empty);
        }
        
        return Result<PagedList<GetOrderListDTO>>.Success(_mapper.Map<PagedList<GetOrderListDTO>>(orderResult.Value));
        
        
    }
}