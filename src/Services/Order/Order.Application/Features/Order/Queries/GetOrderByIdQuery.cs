using MediatR;
using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<GetOrderDetailsDTO>>;