using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Domains.Order.Entities;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.Entity;
using SharedKernel.ValueObjects;

namespace Order.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options)
    {
        
    }

    public DbSet<Domain.Domains.Order.Entities.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddTransactionalOutboxEntities();
        
        modelBuilder.Entity<ProcessedMessage>(builder =>
        {
            builder.HasKey(x => x.MessageId);
            builder.Property(x => x.ProcessedAt).IsRequired();
        });
        
        modelBuilder.Entity<Domain.Domains.Order.Entities.Order>(builder =>
        {
            builder.Property(x=>x.Id).ValueGeneratedOnAdd();
            
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(x=> x.CreatedAt);
            
            builder.OwnsOne(o => o.Total, total =>
            {
                total.Property(t=> t.Amount)
                    .HasColumnName("TotalAmount")
                    .IsRequired();
        
                total
                    .Property(t=> t.Currency)
                    .HasColumnName("TotalCurrency")
                    .HasDefaultValue(CurrencyCode.USD);
            });
            builder.Navigation(n => n.Total).IsRequired();

            builder.OwnsOne(o => o.User, user =>
            {
                user.Property(u=> u.UserId)
                    .HasColumnName(nameof(UserVO.UserId));
        
                user.Property(u => u.UserEmail)
                    .HasColumnName(nameof(UserVO.UserEmail));
        
                user.Property(u=> u.UserName)
                    .HasColumnName(nameof(UserVO.UserName));
            });


            builder.OwnsOne(b => b.DeliveryAddress, address =>
            {
                address.Property(a=> a.Country).HasColumnName(nameof(Address.Country));
        
                address.Property(a => a.State).HasColumnName(nameof(Address.State));
        
                address.Property(a=> a.City).HasColumnName(nameof(Address.City));
        
                address.Property(a=> a.Address1).HasColumnName(nameof(Address.Address1));
        
                address.Property(a=>a.Address2).HasColumnName(nameof(Address.Address2));
        
                address.Property(a=> a.ZipCode).HasColumnName(nameof(Address.ZipCode));

            });
            
        });


        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            
            builder.OwnsOne(b => b.Quantity, quantity =>
            {
                quantity
                    .Property(x=> x.Amount)
                    .HasColumnName("QuantityAmount")
                    .IsRequired();
        
                quantity
                    .Property(x=> x.Uom)
                    .HasColumnName("QuantityUom")
                    .HasDefaultValue(UOM.Piece);
            });

            builder.Navigation(n => n.Quantity).IsRequired();
            
            builder.OwnsOne(o => o.Price, price =>
            {
                price.Property(p=> p.Amount)
                    .HasColumnName("PriceAmount")
                    .IsRequired();
        
                price.Property(t=> t.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasDefaultValue(CurrencyCode.USD);
            });
            builder.Navigation(n=>n.Price).IsRequired();
            
            builder.OwnsOne(b => b.Product, product =>
            {
                product
                    .Property(x => x.ProductId)
                    .HasColumnName(nameof(Domain.Domains.Order.ValueObjects.ProductVO.ProductId))
                    .IsRequired();
        
                product
                    .Property(x => x.ProductName)
                    .HasColumnName(nameof(Domain.Domains.Order.ValueObjects.ProductVO.ProductName))
                    .IsRequired();
        
                
                product
                    .Property(x => x.ProductSku)
                    .HasColumnName(nameof(Domain.Domains.Order.ValueObjects.ProductVO.ProductSku))
                    .IsRequired();
            });
            builder.Navigation(n => n.Product).IsRequired();

        });
        
    }
}