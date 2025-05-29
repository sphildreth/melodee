FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create a non-root user
RUN groupadd -r melodee && useradd -r -g melodee melodee
USER melodee

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Melodee.Blazor/Melodee.Blazor.csproj", "src/Melodee.Blazor/"]
COPY ["src/Melodee.Common/Melodee.Common.csproj", "src/Melodee.Common/"]

# Restore as distinct layers
RUN dotnet restore "src/Melodee.Blazor/Melodee.Blazor.csproj"

# Create directories with proper permissions
RUN mkdir -p /app/storage /app/inbound /app/staging /app/user-images /app/playlists \
    && chown -R melodee:melodee /app

USER melodee
# Copy everything else and build
COPY ["src/Melodee.Blazor/", "src/Melodee.Blazor/"]
COPY ["src/Melodee.Common/", "src/Melodee.Common/"]

WORKDIR "/src/src/Melodee.Blazor"
RUN dotnet build "Melodee.Blazor.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Melodee.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for volumes
RUN mkdir -p /app/storage /app/inbound /app/staging /app/user-images /app/playlists

# Install PostgreSQL client tools for health checks
USER root
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*
USER melodee

# For .NET Core, we use dotnet CLI to run the app
ENTRYPOINT ["dotnet", "server.dll"]
