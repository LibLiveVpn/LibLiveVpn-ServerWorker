# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim AS base
USER root
RUN apt update
RUN apt install -y wireguard
RUN apt install -y iptables
WORKDIR /app


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LibLiveVpn-ServerWorker.AppHost/LibLiveVpn-ServerWorker.AppHost.csproj", "LibLiveVpn-ServerWorker.AppHost/"]
COPY ["LibLiveVpn-ServerWorker.Application/LibLiveVpn-ServerWorker.Application.csproj", "LibLiveVpn-ServerWorker.Application/"]
COPY ["LibLiveVpn-ServerWorker.Infrastructure/LibLiveVpn-ServerWorker.Infrastructure.csproj", "LibLiveVpn-ServerWorker.Infrastructure/"]
COPY ["LibLiveVpn-ServerWorker.Infrastructure.Contracts/LibLiveVpn-ServerWorker.Infrastructure.Contracts.csproj", "LibLiveVpn-ServerWorker.Infrastructure.Contracts/"]
RUN dotnet restore "./LibLiveVpn-ServerWorker.AppHost/LibLiveVpn-ServerWorker.AppHost.csproj"
COPY . .
WORKDIR "/src/LibLiveVpn-ServerWorker.AppHost"
RUN dotnet build "./LibLiveVpn-ServerWorker.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LibLiveVpn-ServerWorker.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibLiveVpn-ServerWorker.AppHost.dll"]