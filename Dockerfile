RUN dotnet build "Melodee.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/Melodee.Blazor"
RUN dotnet publish "Melodee.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports for the Blazor application
EXPOSE 8080
EXPOSE 8081

# Create directories for volumes
RUN mkdir -p /app/storage /app/inbound /app/staging /app/user-images /app/playlists

# Install PostgreSQL client tools for health checks
USER root
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*
USER $APP_UID

ENTRYPOINT ["dotnet", "Melodee.Blazor.dll"]
