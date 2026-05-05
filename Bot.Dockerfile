FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

# Main HTTP
EXPOSE 8080

# Health checks
EXPOSE 8081

# Metrics
EXPOSE 9080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tijdperk.Service.Bot/Tijdperk.Service.Bot.csproj", "Tijdperk.Service.Bot/"]
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
