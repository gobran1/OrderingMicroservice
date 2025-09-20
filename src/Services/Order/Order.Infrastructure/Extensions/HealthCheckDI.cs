using System.Dynamic;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Health.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Order.Infrastructure.Grpc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Order.Infrastructure.Extensions;

public static class HealthCheckDI
{
   public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
   {
      var inventoryUrl = configuration.GetSection("InventoryUrl").Value;
      services.AddGrpcClient<Health.HealthClient>(o=>
         o.Address = new Uri(inventoryUrl ?? String.Empty)
      );


      services.AddHealthChecks()
         .AddCheck("self", () => HealthCheckResult.Healthy("OK"), tags: ["live"], timeout: TimeSpan.FromSeconds(3))
         .AddSqlServer(
            configuration.GetConnectionString("OrderDb"),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"],
            name: "OrderDb-Check",
            timeout: TimeSpan.FromSeconds(3)
         ).AddRabbitMQ(sp =>
            {
               var configuration = sp.GetService<IConfiguration>();
               var connectionFactory = new ConnectionFactory();
               connectionFactory.HostName = configuration["RabbitMq:Host"];
               connectionFactory.UserName = configuration["RabbitMq:Username"];
               connectionFactory.Password = configuration["RabbitMq:Password"];
               connectionFactory.RequestedConnectionTimeout = TimeSpan.FromSeconds(3);

               return connectionFactory.CreateConnectionAsync();

            }, name: "order.rabbitMQ-check", timeout: TimeSpan.FromSeconds(3), tags: ["ready"],
            failureStatus: HealthStatus.Unhealthy
         ).AddCheck<GrpcHealthClient>("inventory-grpc-service-healthCheck", timeout: TimeSpan.FromSeconds(3),
            tags: ["ready"], failureStatus: HealthStatus.Unhealthy)
         .AddCheck("Configuration", () =>
         {
            var requiredConfigurationKeys = new List<string> {
               "ConnectionStrings:OrderDb",
               "RabbitMq:Host",
               "InventoryUrl"
            };
            var emptyFields = requiredConfigurationKeys
               .Where(k => string.IsNullOrEmpty(configuration[k])).ToList();
            
            if (emptyFields.Any())
            {
               return HealthCheckResult.Unhealthy($"required fields are missing from configuration: ({string.Join(',', emptyFields)})");
            }
            
            return HealthCheckResult.Healthy();
            
         }, tags: ["ready"], timeout: TimeSpan.FromSeconds(3));
         ;
      
      return services;
   }

   public static void MapHealthCheckEndpoints(this WebApplication app)
   {
      app.MapHealthChecks("health/live", new HealthCheckOptions
      { 
         Predicate = checks => checks.Tags.Any(tag => tag == "live"),
         ResponseWriter = WriteBasicResponseAsync
      });
      
      app.MapHealthChecks("health/ready", new HealthCheckOptions
      {
         Predicate = checks => checks.Tags.Any(tag => tag == "ready"),
         ResponseWriter = WriteDetailedResponseAsync,
         AllowCachingResponses = false
      });

   }
   
   public static Task WriteBasicResponseAsync(HttpContext httpContext,HealthReport healthReport)
   {
      httpContext.Response.ContentType = "text/plain; charset=utf-8";
      
      httpContext.Response.StatusCode = healthReport.Status == HealthStatus.Healthy ? 200 : 503;
      
      return httpContext.Response.WriteAsync(healthReport.Status.ToString());
   }
   
   public static Task WriteDetailedResponseAsync(HttpContext httpContext, HealthReport healthReport)
   {
      httpContext.Response.ContentType = "application/json; charset=utf-8";
      httpContext.Response.StatusCode = healthReport.Status == HealthStatus.Healthy ? 200 : 503;

      var payload = new
      {
         Status = healthReport.Status,
         TotalDuration = healthReport.TotalDuration,
         Checks = healthReport.Entries.Select(x=> new
         {
            Key = x.Key.ToString(),
            Status = x.Value.Status.ToString(),
            Exception = x.Value.Exception?.Message,
            Duration = x.Value.Duration,
            Description = x.Value.Description
         }).ToList()
      };
      
      
      return httpContext.Response.WriteAsync(JsonSerializer.Serialize<dynamic>(payload, new JsonSerializerOptions{ WriteIndented = true}));
   }
}
