using Inventory.Api.Products.Services;
using Inventory.Infrastructure.Extensions;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration); 

builder.Services.AddControllers();

builder.Services.AddGrpc();
builder.Services.AddHealthCheckServices(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAny"); 


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwagger();
    app.UseSwaggerUI(c=>{
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1");
    });    
}

app.MapGrpcService<ProductServiceImpl>();

app.MapHealthChecksEndpoints();

//app.UseHttpsRedirection();

app.MapControllers();


await InitializeDatabase(app);

app.Run();


async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var inventorySeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    
    dbContext.Database.Migrate();
    await inventorySeeder.SeedAsync();
}
