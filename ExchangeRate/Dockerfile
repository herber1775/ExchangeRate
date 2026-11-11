FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExchangeRate/ExchangeRate.csproj", "ExchangeRate/"]
COPY ["ExchangeRate.Core/ExchangeRate.Core.csproj", "ExchangeRate.Core/"]
COPY ["ExchangeRate.Infrastructure/ExchangeRate.Infrastructure.csproj", "ExchangeRate.Infrastructure/"]
RUN dotnet restore "./ExchangeRate/ExchangeRate.csproj"
COPY . .
WORKDIR "/src/ExchangeRate"
RUN dotnet build "./ExchangeRate.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExchangeRate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRate.dll"]