FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExchangeRate.Web/ExchangeRate.Web.csproj", "ExchangeRate.Web/"]
COPY ["ExchangeRate.Core/ExchangeRate.Core.csproj", "ExchangeRate.Core/"]
RUN dotnet restore "./ExchangeRate.Web/ExchangeRate.Web.csproj"
COPY . .
WORKDIR "/src/ExchangeRate.Web"
RUN dotnet build "./ExchangeRate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExchangeRate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRate.Web.dll"]
