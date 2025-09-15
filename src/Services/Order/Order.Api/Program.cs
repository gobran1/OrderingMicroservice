using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Application.Features.Order.Commands;
using Order.Application.Features.Order.DTOs;
using Order.Application.Features.Order.Queries;
using Order.Infrastructure.Extensions;
using Order.Infrastructure.Persistence;
using SharedKernel.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGrpcReflectionService();
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

MapOrdersRoutes(app);

InitializeDatabase(app);

app.Run();


void InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    
    dbContext.Database.Migrate();
}

void MapOrdersRoutes(WebApplication app)
{
    var orders = app.MapGroup("api/order");

    orders.MapPost("", async ([FromBody]CreateOrderDTO dto,[FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateOrderCommand(dto));

            if (result.IsFailure)
                return Results.BadRequest(result.Error);
    
            var order = result.Value;
    
            return Results.Created($"{order.Id}", order);
        }).Produces<ActionResult<GetOrderDetailsDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    
    orders.MapGet("{id:guid}", async  ([FromRoute]Guid id, [FromServices]IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id));

            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result.Value);
        }).Produces<ActionResult<GetOrderDetailsDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    
    
    orders.MapGet("", async  ([FromQuery] int pageNumber,[FromQuery] int pageSize, [FromServices]IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderPagedListQuery(new PaginationParams{PageNumber = pageNumber, PageSize = pageSize}));

            if (result.IsFailure)
                return Results.BadRequest(result.Error);
            
            return Results.Ok(result.Value);
        }).Produces<ActionResult<PagedList<GetOrderListDTO>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
}
