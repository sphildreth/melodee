FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Melodee.Blazor/Melodee.Blazor.csproj", "src/Melodee.Blazor/"]
RUN dotnet restore "src/Melodee.Blazor/Melodee.Blazor.csproj"
COPY . .
WORKDIR "/src/src/Melodee.Blazor"
RUN dotnet build "Melodee.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Melodee.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for volumes
RUN mkdir -p /app/storage /app/inbound /app/staging /app/user-images /app/playlists

# Install PostgreSQL client tools for health checks (optional)
USER root
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*
USER $APP_UID

ENTRYPOINT ["dotnet", "Melodee.Blazor.dll"]
