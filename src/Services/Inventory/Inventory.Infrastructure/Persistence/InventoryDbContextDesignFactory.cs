using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContextDesignFactory:IDesignTimeDbContextFactory<InventoryDbContext>
{
   
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        optionsBuilder.UseSqlServer("Server=localhost,1401;Database=Inventory;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=true;");

        return new InventoryDbContext(optionsBuilder.Options);
    }
}