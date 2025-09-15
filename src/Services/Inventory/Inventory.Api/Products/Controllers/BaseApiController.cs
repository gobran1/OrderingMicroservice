using Inventory.Application.Features.Product.DTOs;
using Inventory.Application.Features.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Inventory.Api.Products.Controllers;

[ApiController]
public class ProductController:BaseApiController
{
     private readonly IMediator _mediator;

     public ProductController(IMediator mediator)
     {
          _mediator = mediator;
     }
     
     [HttpGet]
     public async Task<ActionResult<PagedList<GetProductDTO>>> GetProducts([FromQuery] PaginationParams paginationParams)
     {
          var result = await _mediator.Send(new GetProductsQuery(paginationParams));
    
          if (result.IsFailure)
               return BadRequest(result.Error);
    
          return Ok(result.Value);
     }
}