FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create a non-root user
RUN groupadd -r melodee && useradd -r -g melodee melodee
USER melodee

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install EF Core tools globally in the build stage where SDK is available
RUN dotnet tool install --global dotnet-ef --version 9.0.5

# Copy Directory.Packages.props first for central package management
COPY ["Directory.Packages.props", "./"]

# Copy project files
COPY ["src/Melodee.Blazor/Melodee.Blazor.csproj", "src/Melodee.Blazor/"]
COPY ["src/Melodee.Common/Melodee.Common.csproj", "src/Melodee.Common/"]

# Restore as distinct layers
RUN dotnet restore "src/Melodee.Blazor/Melodee.Blazor.csproj"

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

# Create directories for volumes as root, then change ownership
USER root
RUN mkdir -p /app/storage /app/inbound /app/staging /app/user-images /app/playlists && \
    chown -R melodee:melodee /app

# Install PostgreSQL client tools for health checks
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Copy dotnet-ef tool from build stage
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools

# Add dotnet tools to PATH for both root and melodee user
ENV PATH="$PATH:/root/.dotnet/tools:/home/melodee/.dotnet/tools"

# Copy the source code needed for migrations (EF Core needs the project files)
COPY --from=build /src/src/Melodee.Common/ /app/src/Melodee.Common/
COPY --from=build /src/src/Melodee.Blazor/ /app/src/Melodee.Blazor/
COPY --from=build /src/Directory.Packages.props /app/

# Create migration script
RUN echo '#!/bin/bash\n\
set -e\n\
echo "Starting database migration..."\n\
# Wait for database to be ready\n\
until pg_isready -h melodee-db -p 5432 -U melodeeuser; do\n\
  echo "Waiting for database..."\n\
  sleep 2\n\
done\n\
echo "Database is ready"\n\
echo "Starting application..."\n\
cd /app\n\
exec dotnet server.dll' > /app/migrate-and-start.sh && \
    chmod +x /app/migrate-and-start.sh

# Switch back to melodee user
USER melodee

# Use the migration script as entrypoint
ENTRYPOINT ["/app/migrate-and-start.sh"]
