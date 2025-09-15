using Inventory.Domain.Domains.Catalog.Entities;
using Inventory.Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entity;
using SharedKernel.ValueObjects;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options): base(options)
    {
    }   
    public DbSet<Product> Products { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddTransactionalOutboxEntities();
        
        modelBuilder.Entity<ProcessedMessage>(builder =>
        {
            builder.HasKey(x=>x.MessageId);
            builder.Property(x => x.ProcessedAt).IsRequired();
        });
        
        modelBuilder.Entity<Product>(builder =>
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            builder.HasIndex(x=>x.CreatedAt);
            
            builder.Property(x => x.Name).IsRequired();
            builder.HasIndex(x => x.Name);

            builder.Property(x => x.Sku);
            builder.HasIndex(x => x.Sku).IsUnique();
            

            builder.OwnsOne(b => b.Price, price =>
            {
                price
                    .Property(x => x.Amount)
                    .HasColumnName("PriceAmount")
                    .IsRequired();

                price
                    .Property(x => x.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasDefaultValue(CurrencyCode.USD);
            });

            builder.OwnsOne(b => b.Quantity, quantity =>
            {
                quantity
                    .Property(x => x.Amount)
                    .HasColumnName("QuantityAmount")
                    .IsRequired();
        
                quantity
                    .Property(x => x.Uom)
                    .HasColumnName("QuantityUom")
                    .HasDefaultValue(UOM.Piece);
            });

        });
      
    }
}