using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Order.Application.Common.Behaviors;
using Order.Application.Common.Interfaces;
using Order.Application.Common.Mappings;
using Order.Application.Features;
using Order.Application.Features.Order.Repositories;
using Order.Infrastructure.Consumer;
using Order.Infrastructure.DomainEvent;
using Order.Infrastructure.Grpc;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Repository;
using Polly;
using Product.V1;
using SharedKernel.Entity;
using SharedKernel.Messaging;
using SharedKernel.Repositories;

namespace Order.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEfDbContext(configuration);
        services.AddGrpcClient(configuration);
        services.AddMessageBus(configuration);
        services.AddDbRepositories(configuration);
        services.AddSwagger(configuration);
        
        services.AddScoped<IMessageBus, MessageBus>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRDependencyMarker).Assembly));
        
        services.AddAutoMapper(typeof(MapperProfile));
        
        services.AddValidatorsFromAssembly(typeof(MediatRDependencyMarker).Assembly);
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }

   
    
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

            c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
                In = ParameterLocation.Header, Description = "JWT Authorization header"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" } },
                    new string[] { }
                }

            });
        });
        return services;
    }
    public static IServiceCollection AddGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        var inventoryUrl = configuration.GetSection("InventoryUrl").Value;

        services.AddGrpcClient<ProductService.ProductServiceClient>(o=>
                o.Address = new Uri(inventoryUrl ?? String.Empty)
            ).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler())
            .AddTransientHttpErrorPolicy(p=>p.WaitAndRetryAsync([
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5)
            ]))
            .AddTransientHttpErrorPolicy(p=> p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)));

        services.AddScoped<IProductGrpcClient, ProductGrpcClient>();
       
        services.AddGrpc();
        services.AddGrpcReflection();
        
        return services;
    }
    
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
                o.UseSqlServer();
            });
            
            x.AddConsumer<SalesStockFailedConsumer>();
            x.AddConsumer<SalesStockReservedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? string.Empty);
                    h.Password(configuration["RabbitMq:Password"] ?? string.Empty);
                });
                
                cfg.ReceiveEndpoint("order.sales-stock-reserved.v1", e =>
                {
                    e.ConfigureConsumer<SalesStockReservedConsumer>(context);
                });
                
                cfg.ReceiveEndpoint("order.sales-stock-failed.v1", e =>
                {
                    e.ConfigureConsumer<SalesStockFailedConsumer>(context);
                });
            });
        });
        
        return services;
    }
    
    public static IServiceCollection AddEfDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("OrderDb"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                sqlOptions.CommandTimeout(30);
                
                sqlOptions.UseCompatibilityLevel(120);
            })
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(configuration.GetValue<bool>("EnableSensitiveDataLogging", false))
            .EnableDetailedErrors(configuration.GetValue<bool>("EnableDetailedErrors", false))
            .EnableThreadSafetyChecks(false);
        });
        
        return services;
    }
    
    public static IServiceCollection AddDbRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        
        return services;
    }
    
}