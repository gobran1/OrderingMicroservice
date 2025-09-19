using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Grpc;

public class GrpcHealthClient:IHealthCheck
{
    private readonly Health.HealthClient _healthClient;
    private readonly ILogger<GrpcHealthClient> _logger;
    private readonly TimeSpan _deadline = TimeSpan.FromSeconds(2);
    
    
    public GrpcHealthClient(Health.HealthClient healthClient,ILogger<GrpcHealthClient> logger)
    {
        _healthClient = healthClient;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
         var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
         cts.CancelAfter(_deadline);

         try
         {
             var reply = await _healthClient.CheckAsync(new HealthCheckRequest(), cancellationToken: cancellationToken);

             if (reply.Status == HealthCheckResponse.Types.ServingStatus.Serving)
             {
                 return HealthCheckResult.Healthy("Grpc healthy");
             }

             return HealthCheckResult.Unhealthy($"Grpc unhealthy {reply.Status}");
         }
         catch (Exception e)
         {
             _logger.LogError(e.Message);
             return HealthCheckResult.Unhealthy($"Grpc Exception during sending call to inventory {e.Message}",e);
         }


    }
}