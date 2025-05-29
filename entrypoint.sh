#!/bin/bash
set -e

echo "Fixing volume permissions for melodee user..."
chown -R melodee:melodee /app/storage /app/inbound /app/staging /app/user-images /app/playlists
chmod -R 755 /app/storage /app/inbound /app/staging /app/user-images /app/playlists
echo "Permissions fixed!"

echo "Starting application as melodee user..."
exec su melodee -c 'sh -c "echo \"Starting container...\"; echo \"Container environment:\"; env | grep -E \"(DB|CONNECTION)\" || true; echo \"Testing database connectivity...\"; until pg_isready -h melodee-db -p 5432 -U melodeeuser -d melodeedb; do echo \"Waiting for database...\"; sleep 2; done && echo \"Database is ready!\" && echo \"Running database migrations...\" && cd /app/src/Melodee.Blazor && dotnet restore && dotnet ef database update --context MelodeeDbContext --connection \"$ConnectionStrings__DefaultConnection\" && echo \"Migrations completed!\" && echo \"Starting application...\" && cd /app && dotnet server.dll"' 