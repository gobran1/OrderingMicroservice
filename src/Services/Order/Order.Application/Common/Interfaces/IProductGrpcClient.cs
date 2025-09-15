using Order.Application.Features.Order.DTOs;
using SharedKernel.DTOs;

namespace Order.Application.Common.Interfaces;

public interface IProductGrpcClient
{
    public Task<Result<List<GetProductDTO>>> GetProductsByIdAsync(List<Guid> ids);
}