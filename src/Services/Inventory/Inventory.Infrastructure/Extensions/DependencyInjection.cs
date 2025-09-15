using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Mappings;
using Inventory.Application.Features;
using Inventory.Application.Features.Product.Repositories;
using Inventory.Infrastructure.Consumers;
using Inventory.Infrastructure.DomainEvent;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Repository;
using Inventory.Infrastructure.Seeding;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SharedKernel.Entity;
using SharedKernel.Messaging;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(configuration);
        services.AddEfDbContext(configuration);
        services.AddMessageBus(configuration);
        services.AddDbRepositories(configuration);
        services.AddSwagger(configuration);
        
        services.AddScoped<IMessageBus, MessageBus>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRDependencyMarker).Assembly));
        
        services.AddAutoMapper(typeof(MapperProfile));
        
        return services;
    }
    
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAny", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

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
    
    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<InventoryDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
                o.UseSqlServer();
            });
            
            x.AddConsumer<OrderCreatedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? string.Empty);
                    h.Password(configuration["RabbitMq:Password"] ?? string.Empty);
                });
                
                cfg.ReceiveEndpoint("inventory.order-created.v1", e =>
                {
                    e.PrefetchCount = 16;
                    e.ConcurrentMessageLimit = 8;
                    e.UseMessageRetry(r=>r.Interval(3,TimeSpan.FromSeconds(5)));
                    e.UseEntityFrameworkOutbox<InventoryDbContext>(context);
                    e.ConfigureConsumer<OrderCreatedConsumer>(context);
                });
                
            });
        });
        
        return services;
    }
    
    private static IServiceCollection AddEfDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("InventoryDb"), sqlOptions =>
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
        
        services.AddScoped<IDataSeeder, InventorySeeder>();
        
        return services;
    }
    
    private static IServiceCollection AddDbRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        
        return services;
    }
    
}
