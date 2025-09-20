using System.Runtime.Intrinsics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Notification.Infrastructure.BackgroundServices;

public class BackgroundHealthCheckService: BackgroundService
{
    private readonly ILogger<BackgroundHealthCheckService> _logger;

    public BackgroundHealthCheckService(ILogger<BackgroundHealthCheckService> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking health... {datetime}", DateTime.UtcNow.ToString());
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}