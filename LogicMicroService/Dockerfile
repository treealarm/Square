#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LogicMicroService/LogicMicroService.csproj", "LogicMicroService/"]
RUN dotnet restore "LogicMicroService/LogicMicroService.csproj"
COPY . .
WORKDIR "/src/LogicMicroService"
RUN dotnet build "LogicMicroService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogicMicroService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogicMicroService.dll"]