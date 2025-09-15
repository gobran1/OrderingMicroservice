using System.ComponentModel.DataAnnotations.Schema;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.Entity;
using SharedKernel.ValueObjects;

namespace Order.Domain.Domains.Order.Entities;

public class OrderItem :Entity<Guid>
{
    public Guid OrderId { get; set; }
    public Quantity Quantity { get; set; }
    public ProductVO Product { get; set; }

    public Money Price { get; set; }
    
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; }


    public OrderItem()
    {
        
    }
    public OrderItem(Guid orderId, Quantity quantity, ProductVO product,Money price)
    {
        OrderId = orderId;
        Quantity = quantity;
        Product = product;
        Price = price;
    }
    
}