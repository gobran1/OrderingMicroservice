using Order.Domain.Domains.Order.Events;
using Order.Domain.Domains.Order.ValueObjects;
using SharedKernel.Entity;
using SharedKernel.ValueObjects;

namespace Order.Domain.Domains.Order.Entities;


public enum OrderStatus
{
    Pending,
    Cancelled,
    Processed,
}

public class Order : AggregateRoot<Guid>
{
    public string OrderNumber { get; set; }
    
    public Address? DeliveryAddress { get; set; }
    
    public UserVO User { get; set; }
    
    public Money Total { get; set; }
    
    public OrderStatus Status { get; set; }
    
    public List<OrderItem> OrderItems { get; set; }

    public Order()
    {
    }
    
    public Order(string orderNumber, Address deliveryAddress, UserVO user, Money total, OrderStatus status, List<OrderItem> orderItems)
    {
        OrderNumber = orderNumber;
        DeliveryAddress = deliveryAddress;
        User = user;
        Total = total;
        Status = status;
        OrderItems = orderItems;
    }
    
    public static Order Create(Address? deliveryAddress, UserVO userVo,List<OrderItem> orderItems)
    {
        var order = new Order();
        order.OrderNumber = Guid.NewGuid().ToString("N");
        order.DeliveryAddress = deliveryAddress;
        order.User = userVo;
        order.Status = OrderStatus.Pending;
        order.OrderItems = orderItems;
        order.Total = new Money(order.CalculateTotal());
        
        order.AddDomainEvent(new OrderCreatedEvent(order));
        
        return order;
    }
    
    public void AddOrderItem(ProductVO productVo,Money price,Quantity quantity)
    {
        OrderItems.Add(
            new OrderItem(
                orderId: Id,
                quantity:  quantity,
                product: productVo,
                price: price
            )
        );
    }
    
    public long CalculateTotal()
    {
        return OrderItems.Sum(i => i.Quantity.Amount * i.Price.Amount);
    }

    public void Fulfill()
    {
        Status = OrderStatus.Processed;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
    }
    
}