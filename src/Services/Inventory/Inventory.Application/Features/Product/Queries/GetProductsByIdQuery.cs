using Inventory.Application.Features.Product.DTOs;
using MediatR;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public record GetProductsByIdQuery(List<string> ProductIds):IRequest<Result<List<GetProductDTO>>>;