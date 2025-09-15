

using Inventory.Domain.Domains.Catalog.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.ValueObjects;

namespace Inventory.Infrastructure.Seeding;

public class InventorySeeder : IDataSeeder
{
    private readonly InventoryDbContext _dbContext;

    public InventorySeeder(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task SeedAsync()
    {
        if (_dbContext.Products.AsNoTracking().Any())
            return;
        
        var products = GetSeedProducts();
        
        await _dbContext.Products.AddRangeAsync(products);
        
        await _dbContext.SaveChangesAsync();
    }

    public static List<Product> GetSeedProducts()
    {
        return new List<Product>
        {
            new Product {  Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with USB receiver", Sku = "ELEC-001", Price = new Money { Amount = 25_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 150, Uom = UOM.Piece } },
            new Product {  Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard with blue switches", Sku = "ELEC-002", Price = new Money { Amount = 75_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 80, Uom = UOM.Piece } },
            new Product {  Name = "Laptop 14-inch", Description = "Lightweight laptop with 8GB RAM and 512GB SSD", Sku = "ELEC-003", Price = new Money { Amount = 899_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 35, Uom = UOM.Piece } },
            new Product {  Name = "Smartphone Pro", Description = "6.5-inch OLED display, 128GB storage, dual SIM", Sku = "ELEC-004", Price = new Money { Amount = 699_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 60, Uom = UOM.Piece } },
            new Product {  Name = "Bluetooth Headphones", Description = "Over-ear headphones with noise cancellation", Sku = "ELEC-005", Price = new Money { Amount = 120_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 100, Uom = UOM.Piece } },
            
            new Product {  Name = "Office Chair", Description = "Adjustable ergonomic office chair with lumbar support", Sku = "FURN-001", Price = new Money { Amount = 199_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 50, Uom = UOM.Piece } },
            new Product {  Name = "Standing Desk", Description = "Electric height adjustable standing desk 120x60cm", Sku = "FURN-002", Price = new Money { Amount = 450_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 25, Uom = UOM.Piece } },
            new Product {  Name = "Bookshelf", Description = "5-tier wooden bookshelf", Sku = "FURN-003", Price = new Money { Amount = 89_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 40, Uom = UOM.Piece } },
            new Product { Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness", Sku = "FURN-004", Price = new Money { Amount = 35_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 200, Uom = UOM.Piece } },
            new Product { Name = "Sofa 3-Seater", Description = "Fabric upholstered 3-seater sofa, grey", Sku = "FURN-005", Price = new Money { Amount = 750_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 10, Uom = UOM.Piece } },
            
            new Product { Name = "T-Shirt", Description = "Cotton crew neck T-shirt, size L", Sku = "CLOT-001", Price = new Money { Amount = 20_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 300, Uom = UOM.Piece } },
            new Product { Name = "Jeans", Description = "Slim-fit denim jeans, size 32", Sku = "CLOT-002", Price = new Money { Amount = 49_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 150, Uom = UOM.Piece } },
            new Product { Name = "Sneakers", Description = "Running sneakers with breathable mesh", Sku = "CLOT-003", Price = new Money { Amount = 89_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 120, Uom = UOM.Piece } },
            new Product { Name = "Winter Jacket", Description = "Waterproof insulated winter jacket", Sku = "CLOT-004", Price = new Money { Amount = 150_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 70, Uom = UOM.Piece } },
            new Product { Name = "Baseball Cap", Description = "Adjustable cotton cap, navy blue", Sku = "CLOT-005", Price = new Money { Amount = 15_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 250, Uom = UOM.Piece } },
            
            new Product { Name = "Coffee Maker", Description = "12-cup programmable coffee maker", Sku = "HOME-001", Price = new Money { Amount = 59_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 90, Uom = UOM.Piece } },
            new Product { Name = "Microwave Oven", Description = "1000W microwave with grill function", Sku = "HOME-002", Price = new Money { Amount = 149_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 40, Uom = UOM.Piece } },
            new Product { Name = "Vacuum Cleaner", Description = "Bagless vacuum cleaner 2000W", Sku = "HOME-003", Price = new Money { Amount = 120_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 60, Uom = UOM.Piece } },
            new Product { Name = "Air Fryer", Description = "Digital air fryer with 4L basket", Sku = "HOME-004", Price = new Money { Amount = 99_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 80, Uom = UOM.Piece } },
            new Product { Name = "Electric Kettle", Description = "1.7L stainless steel electric kettle", Sku = "HOME-005", Price = new Money { Amount = 35_00, Currency = CurrencyCode.USD }, Quantity = new Quantity { Amount = 110, Uom = UOM.Piece } }
        };
    }
}