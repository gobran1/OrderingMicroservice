using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Order.Infrastructure.Persistence;

public class OrderDbContextDesignFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        
        dbContextOptionBuilder.UseSqlServer("Server=localhost,1401;Database=Inventory;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=true;");
        
        return new OrderDbContext(dbContextOptionBuilder.Options);
    }
}