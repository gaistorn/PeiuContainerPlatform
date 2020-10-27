#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["App/DaegunPmsSubscribe/DaegunPmsSubscribe.csproj", "App/DaegunPmsSubscribe/"]
RUN dotnet restore "App/DaegunPmsSubscribe/DaegunPmsSubscribe.csproj"
COPY . .
WORKDIR "/src/App/DaegunPmsSubscribe"
RUN dotnet build "DaegunPmsSubscribe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DaegunPmsSubscribe.csproj" -c Release -o /app/publish

FROM base AS final
RUN mkdir /app/records
RUN chmod 777 /app/records
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DaegunPmsSubscribe.dll"]