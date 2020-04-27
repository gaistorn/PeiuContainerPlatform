#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["App/MqttToRedisWorkerApp/MqttToRedisWorkerApp.csproj", "App/MqttToRedisWorkerApp/"]
COPY ["Library/MqttDataAccessor/MqttDataAccessor.csproj", "Library/MqttDataAccessor/"]
COPY ["Library/RedisDataAccessor/RedisDataAccessor.csproj", "Library/RedisDataAccessor/"]
RUN dotnet restore "App/MqttToRedisWorkerApp/MqttToRedisWorkerApp.csproj"
COPY . .
WORKDIR "/src/App/MqttToRedisWorkerApp"
RUN dotnet build "MqttToRedisWorkerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MqttToRedisWorkerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MqttToRedisWorkerApp.dll"]