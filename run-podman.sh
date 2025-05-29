#!/bin/bash

# Script to run Melodee with Podman Compose using the melodee_podman directories

echo "Starting Melodee with Podman Compose..."
echo "Using directories from /melodee_test/melodee_podman/"

# Create all required directories
mkdir -p /melodee_test/melodee_podman/{storage,inbound,staging,user-images,playlists,db}

# Run Podman Compose with the environment file
podman-compose --env-file podman.env up -d

echo "Melodee is starting up..."
echo "Access it at http://localhost:8080"
echo ""
echo "Useful commands:"
echo "  View logs: podman-compose logs -f"
echo "  Stop: podman-compose down" 