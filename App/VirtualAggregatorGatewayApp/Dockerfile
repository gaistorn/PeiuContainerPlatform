#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["App/VirtualAggregatorGatewayApp/VirtualAggregatorGatewayApp.csproj", "App/VirtualAggregatorGatewayApp/"]
RUN dotnet restore "App/VirtualAggregatorGatewayApp/VirtualAggregatorGatewayApp.csproj"
COPY . .
WORKDIR "/src/App/VirtualAggregatorGatewayApp"
RUN dotnet build "VirtualAggregatorGatewayApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VirtualAggregatorGatewayApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualAggregatorGatewayApp.dll"]