#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8002
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#COPY ["DbLayer/DbLayer.csproj", "DbLayer/"]
#COPY ["Domain/Domain.csproj", "Domain/"]
#COPY ["GrpcDaprLib/GrpcDaprLib.csproj", "GrpcDaprLib/"]
COPY ["LeafletAlarms/LeafletAlarms.csproj", "LeafletAlarms/"]
COPY ["tree_alarm.client/tree_alarm.client.esproj", "tree_alarm.client/"]
#COPY ["PubSubLib/PubSubLib.csproj", "PubSubLib/"]
#COPY ["ValhallaLib/ValhallaLib.csproj", "ValhallaLib/"]

RUN dotnet restore "LeafletAlarms/LeafletAlarms.csproj"
COPY . .
WORKDIR "/src/LeafletAlarms"
RUN dotnet build "LeafletAlarms.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LeafletAlarms.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LeafletAlarms.dll"]