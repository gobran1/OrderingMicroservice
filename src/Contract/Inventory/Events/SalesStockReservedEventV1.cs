using SharedKernel.Messaging;

namespace Contract.Inventory.Events;

public record SalesStockReservedEventV1(Guid OrderId) : IntegrationEvent;