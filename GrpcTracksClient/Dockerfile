#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GrpcTracksClient/GrpcTracksClient.csproj", "GrpcTracksClient/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "GrpcTracksClient/GrpcTracksClient.csproj"
COPY . .
WORKDIR "/src/GrpcTracksClient"
RUN dotnet build "GrpcTracksClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrpcTracksClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrpcTracksClient.dll"]