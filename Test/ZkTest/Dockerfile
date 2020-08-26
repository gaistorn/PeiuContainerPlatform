#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Test/ZkTest/ZkTest.csproj", "Test/ZkTest/"]
RUN dotnet restore "Test/ZkTest/ZkTest.csproj"
COPY . .
WORKDIR "/src/Test/ZkTest"
RUN dotnet build "ZkTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZkTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZkTest.dll"]