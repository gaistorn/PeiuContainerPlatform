#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["App/CollectLatestWeatherApp/CollectLatestWeatherApp.csproj", "App/CollectLatestWeatherApp/"]
COPY ["Library/DatabaseModel/DatabaseModel.csproj", "Library/DatabaseModel/"]
RUN dotnet restore "App/CollectLatestWeatherApp/CollectLatestWeatherApp.csproj"
COPY . .
WORKDIR "/src/App/CollectLatestWeatherApp"
RUN dotnet build "CollectLatestWeatherApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CollectLatestWeatherApp.csproj" -c Release -o /app/publish

ENV ENV_CC_GPS_LAT 36.429409
ENV ENV_CC_GPS_LON 127.390811
ENV ENV_OPENWEATHER_APP_KEY 0e24126ab1639fb0301e58fb0f2a7009

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CollectLatestWeatherApp.dll"]