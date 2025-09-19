using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Inventory.Infrastructure.Extensions;

public static class HealthCheckDI
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self",()=>HealthCheckResult.Healthy("OK"),tags: new[] {"live"},timeout:TimeSpan.FromSeconds(3))
            .AddSqlServer(
                configuration.GetConnectionString("InventoryDb"),
                name: "InventoryDb-check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] {"ready"}
            )
            .AddRabbitMQ(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = cfg["RabbitMq:Host"];
                connectionFactory.UserName = cfg["RabbitMq:Username"];
                connectionFactory.Password = cfg["RabbitMq:Password"];
                connectionFactory.RequestedConnectionTimeout = TimeSpan.FromSeconds(3);
                return connectionFactory.CreateConnectionAsync();
            },name: "RabbitMQ-check",failureStatus: HealthStatus.Unhealthy,tags: new[] {"ready"});


        services.AddGrpcHealthChecks();
        
        return services;
    }

    public static WebApplication MapHealthChecksEndpoints(this WebApplication app)
    {

        app.MapGrpcHealthChecksService();
        
        app.MapHealthChecks("health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteBasicResponseAsync,
        });

        app.MapHealthChecks("health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteDetailedResponseAsync,
            AllowCachingResponses = false,
        });
        
        
        return app;
    }


    public static Task WriteBasicResponseAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "text/plain; charset=utf-8";
        httpContext.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
        
        return httpContext.Response.WriteAsync(report.Status.ToString());
    }

    public static Task WriteDetailedResponseAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        httpContext.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
        
        var payload = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration,
            Checks = report.Entries.Select(x=> new
            {
                Name = x.Key,
                Status = x.Value.Status.ToString(),
                Duration = x.Value.Duration,
                Exception = x.Value.Exception?.ToString(),
                Description = x.Value.Description,
            })
        };


        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload,
                new JsonSerializerOptions { WriteIndented = true })
            );
    }
    
}