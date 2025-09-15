FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Services/Inventory/Inventory.Api/Inventory.Api.csproj", "Services/Inventory/Inventory.Api/"]
COPY ["Services/Inventory/Inventory.Application/Inventory.Application.csproj", "Services/Inventory/Inventory.Application/"]
COPY ["Services/Inventory/Inventory.Domain/Inventory.Domain.csproj", "Services/Inventory/Inventory.Domain/"]
COPY ["Services/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "Services/Inventory/Inventory.Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
COPY ["Contract/Contract.csproj", "Contract/"]

RUN apt-get update && \
    apt-get install -y ca-certificates && \
    rm -rf /var/lib/apt/lists/*

ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
RUN dotnet nuget locals all --clear

RUN dotnet restore "Services/Inventory/Inventory.Api/Inventory.Api.csproj" --no-cache
COPY . .
WORKDIR "/src/Services/Inventory/Inventory.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

COPY ./certs/local-ca.crt /usr/local/share/ca-certificates/local-dev.crt
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates \
 && update-ca-certificates \
 && rm -rf /var/lib/apt/lists/*

RUN useradd --create-home appuser
COPY --chown=appuser:appuser ./certs/aspnetapp.pfx /https/aspnetapp.pfx
RUN chmod 600 /https/aspnetapp.pfx
USER appuser
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=P@ssw0rd!


ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5001;https://+:6001

ENTRYPOINT ["dotnet", "Inventory.Api.dll"]