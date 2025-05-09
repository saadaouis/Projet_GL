# Running EasySave with Docker

## Quick Start (For Users)
If you just want to run EasySave, follow these steps:

1. Make sure Docker is installed on your system
2. Create these directories in your current folder:
   ```bash
   mkdir -p config logs projects
   ```

3. Create a docker-compose.yml file:
   ```yaml
   version: '3.8'
   
   services:
     easysave:
       image: easysave:1.0
       container_name: easysave
       volumes:
         - ./config:/app/config
         - ./logs:/app/logs
         - ./projects:/app/projects
       environment:
         - DOTNET_ENVIRONMENT=Production
         - CONFIG_PATH=/app/config
         - LOG_PATH=/app/logs
       restart: unless-stopped
   ```

4. Run the application:
   ```bash
   docker-compose up -d
   ```

## For Developers
If you want to build the image yourself:

1. Clone the repository:
   ```bash
   git clone [your-repo-url]
   cd easysave
   ```

2. Build and run:
   ```bash
   docker-compose build
   docker-compose up -d
   ```

## Volume Mounts
- `./config`: Configuration files
- `./logs`: Application logs
- `./projects`: Project files

## Environment Variables
- `DOTNET_ENVIRONMENT`: Application environment (Production/Development)
- `CONFIG_PATH`: Path to configuration files
- `LOG_PATH`: Path to log files

## Commands
- Start: `docker-compose up -d`
- Stop: `docker-compose down`
- View logs: `docker-compose logs -f`
- Rebuild: `docker-compose up --build` 