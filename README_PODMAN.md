# Podman Setup

## Quick Start

1. **Set your database password** in `podman.env`
2. **Modify storage paths** in `podman.env`
2. **Run the setup script**:
   ```bash
   ./run-podman.sh
   ```
3. **Access the application** at http://localhost:8080

## What This Does

Maps your configured directories to the Podman containers:
- `storage/` - Processed music library
- `inbound/` - Incoming music files  
- `staging/` - Processing staging area
- `user-images/` - User profile images
- `playlists/` - Playlist data
- `db/` - Database files

## Commands

```bash
# Start
./run-podman.sh

# View logs
podman-compose logs -f

# Stop
podman-compose down
``` 