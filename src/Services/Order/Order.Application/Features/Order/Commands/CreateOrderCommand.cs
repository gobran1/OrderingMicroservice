using MediatR;
using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Features.Order.Commands;

public record CreateOrderCommand(CreateOrderDTO CreateOrderDto):IRequest<Result<GetOrderDetailsDTO>>;
