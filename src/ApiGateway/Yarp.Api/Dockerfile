FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


RUN apt-get update && \
    apt-get install -y ca-certificates && \
    rm -rf /var/lib/apt/lists/*

ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

RUN dotnet nuget locals all --clear


RUN mkdir -p /tmp/certs
RUN dotnet dev-certs https -ep /tmp/certs/aspnetapp.pfx -p password --trust

COPY ["/ApiGateway/Yarp.Api/Yarp.Api.csproj", "ApiGateway/Yarp.Api/"]
RUN dotnet restore "ApiGateway/Yarp.Api/Yarp.Api.csproj"
COPY . .
WORKDIR "/src/ApiGateway/Yarp.Api"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yarp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
COPY  /ApiGateway/Yarp.Api/healthcheck.sh /app
COPY ./certs/local-ca.crt /usr/local/share/ca-certificates/local-dev.crt

RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates curl \
 && update-ca-certificates \
 && rm -rf /var/lib/apt/lists/*

RUN useradd --create-home appuser
COPY --chown=appuser:appuser ./certs/aspnetapp.pfx /https/aspnetapp.pfx
RUN chmod 600 /https/aspnetapp.pfx

RUN chmod +x /app/healthcheck.sh

USER appuser

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=P@ssw0rd!
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Yarp.Api.dll"]
