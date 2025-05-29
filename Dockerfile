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

# Final image - use SDK instead of runtime to support EF migrations
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install PostgreSQL client tools, network debugging tools, and nano editor
RUN apt-get update && \
    apt-get install -y \
        postgresql-client \
        iputils-ping \
        netcat-openbsd \
        nano \
        && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Copy the source code needed for migrations (EF Core needs the project files)
COPY --from=build /src/src/Melodee.Common/ /app/src/Melodee.Common/
COPY --from=build /src/src/Melodee.Blazor/ /app/src/Melodee.Blazor/
COPY --from=build /src/Directory.Packages.props /app/

# Create a non-root user and switch to it
RUN groupadd -r melodee && useradd -r -g melodee -m melodee

# Change ownership of the app directory to melodee user
RUN chown -R melodee:melodee /app

# Set dotnet environment variables to avoid permission issues
ENV DOTNET_CLI_HOME="/home/melodee"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV HOME="/home/melodee"

USER melodee

# Install EF Core tools globally for the melodee user
RUN dotnet tool install --global dotnet-ef --version 9.0.5

# Add tools to PATH
ENV PATH="$PATH:/home/melodee/.dotnet/tools"

# Run database migration and start the application
CMD ["sh", "-c", "echo 'Starting container...'; echo 'Container environment:'; env | grep -E '(DB|CONNECTION)' || true; echo 'Testing database connectivity...'; until pg_isready -h melodee-db -p 5432 -U melodeeuser -d melodeedb; do echo 'Waiting for database...'; sleep 2; done && echo 'Database is ready!' && echo 'Running database migrations...'; cd /app/src/Melodee.Blazor && dotnet restore && dotnet ef database update --context MelodeeDbContext --connection \"$ConnectionStrings__DefaultConnection\" && echo 'Migrations completed!' && echo 'Starting application...' && cd /app && dotnet server.dll"]
