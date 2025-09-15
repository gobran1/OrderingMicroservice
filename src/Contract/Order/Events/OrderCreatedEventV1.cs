using System.Security.AccessControl;
using SharedKernel.Messaging;
using SharedKernel.ValueObjects;

namespace Contract.Order.Events;

public record OrderCreatedEventV1 : IntegrationEvent
{
    public Guid OrderId { get; set; }

    public List<OrderCreatedEventV1Items> Items { get; set; }
};
public class OrderCreatedEventV1Items 
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Quantity Quantity { get; set; }
}