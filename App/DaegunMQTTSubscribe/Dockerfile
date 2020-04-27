#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["App/DaegunMQTTSubscribe/DaegunMQTTSubscribe.csproj", "App/DaegunMQTTSubscribe/"]
COPY ["Library/EventPublisher/EventPublisher.csproj", "Library/EventPublisher/"]
COPY ["Library/EventModel/EventModel.csproj", "Library/EventModel/"]
COPY ["Library/MqttDataAccessor/MqttDataAccessor.csproj", "Library/MqttDataAccessor/"]
COPY ["Library/RedisDataAccessor/RedisDataAccessor.csproj", "Library/RedisDataAccessor/"]
RUN dotnet restore "App/DaegunMQTTSubscribe/DaegunMQTTSubscribe.csproj"
COPY . .
WORKDIR "/src/App/DaegunMQTTSubscribe"
RUN dotnet build "DaegunMQTTSubscribe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaegunMQTTSubscribe.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DaegunMQTTSubscribe.dll"]