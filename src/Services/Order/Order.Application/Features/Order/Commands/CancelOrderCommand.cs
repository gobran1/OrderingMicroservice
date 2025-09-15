using MediatR;
using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Commands;

public record CancelOrderCommand(Guid OrderId):IRequest<Result<GetOrderDetailsDTO>>;
