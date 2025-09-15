using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Consumer;

namespace Notification.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
       services.AddMessageBus(configuration);
        return services;
    }
    
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SalesStockFailedConsumer>();
            x.AddConsumer<SalesStockReservedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? string.Empty);
                    h.Password(configuration["RabbitMq:Password"] ?? string.Empty);
                });
                
                cfg.ReceiveEndpoint("notification.sales-stock-reserved.v1", e =>
                {
                    e.ConfigureConsumer<SalesStockReservedConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("notification.sales-stock-failed.v1", e =>
                {
                    e.ConfigureConsumer<SalesStockFailedConsumer>(context);
                });
            });
        });
        
        return services;
    }
    
    
}