FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

# Main HTTP
EXPOSE 8080

# Health checks
EXPOSE 8081

# Metrics
EXPOSE 9080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project
COPY ["Tijdperk.Service.Bot/Tijdperk.Service.Bot.csproj", "Tijdperk.Service.Bot/"]

# Copy dependencies
COPY ["Tijdperk.Logic/Tijdperk.Logic.csproj", "Tijdperk.Logic/"]
COPY ["Tijdperk.Data.Postgres/Tijdperk.Data.Postgres.csproj", "Tijdperk.Data.Postgres/"]
COPY ["Tijdperk.Common.Data/Tijdperk.Common.Data.csproj", "Tijdperk.Common.Data/"]
COPY ["Tijdperk.Common.Core/Tijdperk.Common.Core.csproj", "Tijdperk.Common.Core/"]

RUN dotnet restore "Tijdperk.Service.Bot/Tijdperk.Service.Bot.csproj"
COPY . .
WORKDIR "/src/Tijdperk.Service.Bot"
RUN dotnet build "./Tijdperk.Service.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tijdperk.Service.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tijdperk.Service.Bot.dll"]
