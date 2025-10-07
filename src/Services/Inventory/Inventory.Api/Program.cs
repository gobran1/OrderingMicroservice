using System.Diagnostics;
using Inventory.Api.Products.Services;
using Inventory.Infrastructure.Extensions;
using Inventory.Infrastructure.Monitoring;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Platform.Observability;
using Serilog;
using Serilog.Formatting.Compact;


var serviceName = "Inventory";
var version = "1.0.0";
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("service",serviceName)
    .Enrich.WithProperty("version",version)
    .Enrich.WithProperty("environment",environment)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.With<TraceEnricher>()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting {ServiceName}",serviceName);
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(logger);
    
    builder.Services.AddOpenApi();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();

    builder.Services.AddGrpc();
    builder.Services.AddHealthCheckServices(builder.Configuration);
    
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resourceBuilder =>
            resourceBuilder.AddService(serviceName: serviceName, serviceVersion: version))
        .WithTracing(traceProviderBuilder =>
        {
            traceProviderBuilder.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(serviceName)
                .AddSource("Inventory.Application")
                .AddSource("Inventory.Infrastructure")
                .SetSampler(new AlwaysOnSampler())
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = new Uri("http://otel-collector:4318/v1/traces");
                    configure.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
        }).WithMetrics(meterProviderBuilder =>
        {
            meterProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("business.metrics")
                .AddPrometheusExporter();
        });
    
    var app = builder.Build();
    
    app.UseCors("AllowAny");
    
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1"); });
    }

    app.MapGrpcService<ProductServiceImpl>();

    app.MapHealthChecksEndpoints();

    app.MapPrometheusScrapingEndpoint("/metrics");
    
//app.UseHttpsRedirection();

    app.MapControllers();
    
    await InitializeDatabase(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex,"Service stopped unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var inventorySeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    
    dbContext.Database.Migrate();
    await inventorySeeder.SeedAsync();
}
