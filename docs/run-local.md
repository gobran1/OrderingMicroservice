//some of used commands to create the strcuture

[//]: # (setup the order service project structure)
cd src/Services/Order
dotnet new classlib -n Order.Domain
dotnet new classlib -n Order.Application
dotnet new classlib -n Order.Infrastructure
dotnet new webapi -n Order.Api
dotnet new sln
dotnet sln add Order.Domain
dotnet sln add Order.Application
dotnet sln add -n Order.Infrastructure
dotnet sln add Order.Api
dotnet add Order.Application reference Order.Domain
dotnet add Order.Infrastructure reference Order.Application
dotnet add Order.Api reference Order.Application
dotnet add Order.Api reference Order.Infrastructure

[//]: # (setup the Inventory service project structure)
cd src/Services/Inventory
dotnet new classlib -n Inventory.Domain
dotnet new classlib -n Inventory.Application
dotnet new classlib -n Inventory.Infrastructure
dotnet new webapi -n Inventory.Api
dotnet new sln
dotnet sln add Inventory.Domain
dotnet sln add Inventory.Application
dotnet sln add -n Inventory.Infrastructure
dotnet sln add Inventory.Api
dotnet add Inventory.Application reference Inventory.Domain
dotnet add Inventory.Infrastructure reference Inventory.Application
dotnet add Inventory.Api reference Inventory.Application
dotnet add Inventory.Api reference Inventory.Infrastructure

[//]: # (setup the Notification service project structure)
cd src/Services/Notification
dotnet new classlib -n Notification.Domain
dotnet new classlib -n Notification.Application
dotnet new classlib -n Notification.Infrastructure
dotnet new worker -n Notification.Api
dotnet new sln
dotnet sln add Notification.Domain
dotnet sln add Notification.Application
dotnet sln add -n Notification.Infrastructure
dotnet sln add Notification.Api
dotnet add Notification.Application reference Notification.Domain
dotnet add Notification.Infrastructure reference Notification.Application
dotnet add Notification.Api reference Notification.Application
dotnet add Notification.Api reference Notification.Infrastructure


//commands to create migrations

dotnet ef migrations add InitialCreate \
--project src/Services/Order/Order.Infrastructure/Order.Infrastructure.csproj \
--startup-project src/Services/Order/Order.Api/Order.Api.csproj \
--output-dir Persistence/EFMigrations

dotnet ef migrations add InitialCreate \
--project src/Services/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj \
--startup-project src/Services/Inventory/Inventory.Api/Inventory.Api.csproj \
--output-dir Persistence/EFMigrations


