Project Documentation
Running the Project

There are two ways to run the project:

1. Local development

Run SQL Server and RabbitMQ in containers, then start each microservice individually (outside Docker).
In this mode, the API Gateway will be available at:
https://localhost:5000

cd src
docker-compose -f docker-compose.dev.yml up db -d
docker-compose -f docker-compose.dev.yml up rabbitmq -d

After that, run each service from your IDE or command line

2. Full Docker environment

Run all services (including the gateway, APIs, and dependencies) inside containers:

cd src
docker-compose -f docker-compose.dev.yml up -d

In this mode, the API Gateway will be available at: 
http://localhost:5000

Project Flow:

User creates an order.

The CreateOrderCommand endpoint is invoked.

Validation is handled by CreateOrderCommandValidator.

Business logic is executed by CreateOrderCommandHandler.

Inside the handler, the order is created.

To calculate the total amount, a gRPC call is made to the Inventory Service to fetch product pricing.

Once created, an OrderCreated domain event is raised.

The OrderCreatedHandler adds an integration event (OrderCreated.v1) into the MassTransit EF Outbox tables.

The order data is persisted in the database.

The MassTransit hosted service fetches outbox messages from the database and publishes them to RabbitMQ.

The Inventory Service consumes the event on the inventory.order-created.v1 endpoint:

If stock is available, it reserves it and it publishes a new integration event marking the order as Fulfilled.

If stock is unavailable, it publishes a new integration event marking the order as Cancelled.

These integration events are also persisted via the EF Outbox before being dispatched.

The Order Service consumes the stock reservation events:

Updates the order status to Processed (success) or Cancelled (failure).

The Notification Service also consumes these events:

log notifications to console to inform the latest order status.