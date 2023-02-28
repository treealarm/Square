#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RouterMicroService/RouterMicroService.csproj", "RouterMicroService/"]
COPY ["DbLayer/DbLayer.csproj", "DbLayer/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Router/Router.csproj", "Router/"]
RUN dotnet restore "RouterMicroService/RouterMicroService.csproj"
COPY . .
WORKDIR "/src/RouterMicroService"
RUN dotnet build "RouterMicroService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RouterMicroService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RouterMicroService.dll"]