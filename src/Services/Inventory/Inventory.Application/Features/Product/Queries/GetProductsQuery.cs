using Inventory.Application.Features.Product.DTOs;
using MediatR;
using SharedKernel.DTOs;

namespace Inventory.Application.Features.Product.Queries;

public record GetProductsQuery(PaginationParams paginationParams):IRequest<Result<PagedList<GetProductDTO>>>;