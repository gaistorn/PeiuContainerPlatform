#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Device/Power21.Device/Power21.Device.csproj", "Device/Power21.Device/"]
RUN dotnet restore "Device/Power21.Device/Power21.Device.csproj"
COPY . .
WORKDIR "/src/Device/Power21.Device"
RUN dotnet build "Power21.Device.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Power21.Device.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Power21.Device.dll"]
