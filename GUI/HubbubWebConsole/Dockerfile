#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["GUI/HubbubWebConsole/HubbubWebConsole.csproj", "GUI/HubbubWebConsole/"]
RUN dotnet restore "GUI/HubbubWebConsole/HubbubWebConsole.csproj"
COPY . .
WORKDIR "/src/GUI/HubbubWebConsole"
RUN dotnet build "HubbubWebConsole.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HubbubWebConsole.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HubbubWebConsole.dll"]