FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Notification/Notification.Api/Notification.Api.csproj", "Services/Notification/Notification.Api/"]
COPY ["Services/Notification/Notification.Application/Notification.Application.csproj", "Services/Notification/Notification.Application/"]
COPY ["Services/Notification/Notification.Domain/Notification.Domain.csproj", "Services/Notification/Notification.Domain/"]
COPY ["Services/Notification/Notification.Infrastructure/Notification.Infrastructure.csproj", "Services/Notification/Notification.Infrastructure/"]
COPY ["Contract/Contract.csproj", "Contract/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]


RUN apt-get update && \
    apt-get install -y ca-certificates && \
    rm -rf /var/lib/apt/lists/*

ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

RUN dotnet nuget locals all --clear

RUN dotnet restore "Services/Notification/Notification.Api/Notification.Api.csproj"
COPY . .
WORKDIR "/src/Services/Notification/Notification.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

RUN useradd --create-home appuser
USER appuser

ENTRYPOINT ["dotnet", "Notification.Api.dll"]
