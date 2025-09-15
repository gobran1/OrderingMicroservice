using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Products.Controllers;

[ApiController]
[Route("api/inventory/[controller]")]
public class BaseApiController : ControllerBase
{
}