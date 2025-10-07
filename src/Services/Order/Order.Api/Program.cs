using System.Diagnostics;
using Contract.SharedDTOs;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Application.Features.Order.Commands;
using Order.Application.Features.Order.DTOs;
using Order.Application.Features.Order.Queries;
using Order.Infrastructure.Extensions;
using Order.Infrastructure.Monitoring;
using Order.Infrastructure.Persistence;
using Platform.Observability;
using Serilog;
using Serilog.Formatting.Compact;
using SharedKernel.DTOs;
using SharedKernel.ValueObjects;


var serviceName = "Order.Api";
var serviceVersion = "1.0.0";
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

// Create ActivitySource for tracing
var activitySource = new ActivitySource(serviceName, serviceVersion);

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("service", serviceName)
    .Enrich.WithProperty("version", serviceVersion)
    .Enrich.WithProperty("environment", environment)
    .Enrich.FromLogContext()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.With<TraceEnricher>()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting {ServiceName}", serviceName);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(loggerConfiguration);

    builder.Services.AddOpenApi();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHealthCheckServices(builder.Configuration);

    
    // Configure OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .WithTracing(providerBuilder =>
        {
            providerBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(serviceName)
                .SetSampler(new AlwaysOnSampler())
                .AddOtlpExporter(options => 
                {
                    options.Endpoint = new Uri("http://otel-collector:4318/v1/traces");
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
        })
        .WithMetrics(providerBuilder =>
        {
            providerBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("business.metrics")
                .AddPrometheusExporter();
        });


    var app = builder.Build();


    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapGrpcReflectionService();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

//app.UseHttpsRedirection();
    
    app.MapPrometheusScrapingEndpoint("/metrics");
    
    app.MapHealthCheckEndpoints();
    
    MapOrdersRoutes(app);
    
    app.MapGet("api/order/test-logging", () =>
    {
        using var activity = activitySource.StartActivity("test-logging");
        activity?.SetTag("test", "true");
        activity?.SetTag("endpoint", "test-logging");
        
        Log.Information("Testing logging executed with traceId={TraceId}", Activity.Current?.TraceId.ToString() ?? "no-trace");
        
        activity?.SetStatus(ActivityStatusCode.Ok);
        
        return Results.Ok(new { traceId = Activity.Current?.TraceId.ToString() });
    });
    
    InitializeDatabase(app);

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Service Closed Unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

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
