#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs

WORKDIR /src
COPY ["LeafletAlarms/LeafletAlarms.csproj", "LeafletAlarms/"]
COPY ["DbLayer/DbLayer.csproj", "DbLayer/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Router/Router.csproj", "Router/"]
RUN dotnet restore "LeafletAlarms/LeafletAlarms.csproj"
COPY . .
WORKDIR "/src/LeafletAlarms"
RUN dotnet build "LeafletAlarms.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeafletAlarms.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LeafletAlarms.dll"]