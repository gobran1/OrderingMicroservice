using SharedKernel.Messaging;

namespace Contract.Inventory.Events;

public record SalesStockFailedEventV1(Guid OrderId) : IntegrationEvent;