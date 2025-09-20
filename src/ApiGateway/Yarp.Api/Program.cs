using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpForwarder();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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


builder.Services.AddHealthChecks()
    .AddCheck("self",()=>HealthCheckResult.Healthy(),timeout:TimeSpan.FromSeconds(3),tags:["live"]);

var app = builder.Build();

app.UseCors("AllowAny");

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/inventory/swagger/v1/swagger.json", "Inventory API v1"); 
        c.SwaggerEndpoint("/api/order/swagger/v1/swagger.json", "Order API v1");
    });
}

//app.UseHttpsRedirection();

app.MapReverseProxy();

app.MapGet("/", () => "YARP gateway running");
app.MapHealthChecks("health/live",new HealthCheckOptions
{
    Predicate = predicate => predicate.Tags.Contains("live"),
    AllowCachingResponses = false,
    ResponseWriter = (HttpContext httpContext, HealthReport report) =>
    {
        httpContext.Response.ContentType = "text/plain; charset=utf-8";
        httpContext.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
        return httpContext.Response.WriteAsync(report.Status.ToString());
    }
});


app.Run();
