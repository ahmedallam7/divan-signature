# Docker Deployment Guide

This guide explains how to deploy the UUNATEK API with SQL Server to Linux using Docker (no Linux experience required!).

## Prerequisites

On your Linux server, you need:
- Docker installed
- Docker Compose installed (usually comes with Docker)

To check if they're installed, run:
```bash
docker --version
docker-compose --version
```

## What is Docker?

Docker packages your application with everything it needs to run (code, runtime, libraries) into a container. Think of it like a standardized shipping container - it works the same way everywhere, whether on Windows, Linux, or Mac.

## What's Included?

This Docker setup includes:
- **Your .NET API** - UUNATEK API running on port 5000
- **SQL Server 2022** - Fully configured database on port 1433
- **Automatic Database Setup** - Database and tables are created automatically on first run
- **Data Persistence** - Database data is saved even if containers restart

Everything is self-contained. No external dependencies needed!

## Files Created

1. **Dockerfile** - Instructions for building your app into a Docker image
2. **.dockerignore** - Tells Docker which files to skip (like build outputs)
3. **docker-compose.yml** - Runs both API and SQL Server together
4. **appsettings.Production.json** - Production configuration (baked into container)

## Deployment Steps

### Simple Deployment (Recommended)

1. **Copy files to your Linux server:**
   ```bash
   # From your Windows machine, copy everything to Linux server
   # Replace 'user' and 'server-ip' with your actual values
   scp -r . user@server-ip:/home/user/uunatek-api
   ```

2. **SSH into your Linux server:**
   ```bash
   ssh user@server-ip
   cd /home/user/uunatek-api
   ```

3. **Build and run everything:**
   ```bash
   docker-compose up -d
   ```
   
   This single command will:
   - Pull SQL Server 2022 image
   - Build your API
   - Start SQL Server
   - Wait for SQL Server to be ready
   - Start your API
   - Create database and run migrations automatically

4. **Check if it's working:**
   ```bash
   # Check all containers are running
   docker ps
   
   # Test the API
   curl http://localhost:5000
   
   # View API logs
   docker logs uunatek-api
   
   # View SQL Server logs
   docker logs uunatek-sqlserver
   ```

That's it! Your API and database are now running!

## Useful Commands

### Check if containers are running:
```bash
docker ps
# Should show both uunatek-api and uunatek-sqlserver
```

### View logs:
```bash
# API logs
docker logs uunatek-api

# SQL Server logs
docker logs uunatek-sqlserver

# All logs together
docker-compose logs
```

### View live logs:
```bash
# Follow API logs
docker logs -f uunatek-api

# Follow SQL Server logs
docker logs -f uunatek-sqlserver

# Follow all logs
docker-compose logs -f
```

### Stop everything:
```bash
docker-compose down
```

### Stop but keep data:
```bash
docker-compose down
# Data in sqlserver_data volume is preserved
```

### Stop and remove all data:
```bash
docker-compose down -v
# Warning: This deletes the database!
```

### Restart containers:
```bash
docker-compose restart

# Or restart individually
docker restart uunatek-api
docker restart uunatek-sqlserver
```

### Update after code changes:
```bash
# Copy new files to server, then:
docker-compose down
docker-compose up -d --build
# SQL Server data is preserved automatically
```

## Configuration

### SQL Server Password

The default SQL Server password is set in `docker-compose.yml`:
```yaml
MSSQL_SA_PASSWORD=YourStrong!Passw0rd
```

**IMPORTANT for Production:** Change this password before deploying!

1. Edit `docker-compose.yml` and change `YourStrong!Passw0rd` in TWO places:
   - Under `sqlserver` service in `MSSQL_SA_PASSWORD`
   - Under `uunatek-api` service in `ConnectionStrings__DefaultConnection`

2. Also update the healthcheck password in the `sqlserver` service

### Environment Variables

You can add more environment variables in `docker-compose.yml` under the `environment` section:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - YourCustomSetting=value
```

### Port Configuration

By default:
- API runs on port **5000**
- SQL Server runs on port **1433**

To change ports, edit `docker-compose.yml`:

```yaml
ports:
  - "8080:8080"  # Change first number to desired host port
```

### Database Persistence

Database data is automatically saved in a Docker volume named `sqlserver_data`. This means:
- Data survives container restarts
- Data survives `docker-compose down`
- Data is only deleted with `docker-compose down -v`

To backup the database:
```bash
# Backup database to file
docker exec uunatek-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "BACKUP DATABASE DiwanPrinter TO DISK='/var/opt/mssql/backup/DiwanPrinter.bak'" -C

# Copy backup file from container
docker cp uunatek-sqlserver:/var/opt/mssql/backup/DiwanPrinter.bak ./DiwanPrinter.bak
```

## Troubleshooting

### Containers won't start:
```bash
# Check all container statuses
docker ps -a

# Check logs for errors
docker logs uunatek-api
docker logs uunatek-sqlserver
```

### API can't connect to database:
```bash
# Check if SQL Server is healthy
docker inspect uunatek-sqlserver | grep Health

# Try connecting manually to SQL Server
docker exec -it uunatek-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION" -C
```

### Database not created:
The database is created automatically by EF migrations when the API starts. Check API logs:
```bash
docker logs uunatek-api | grep -i migration
```

### Check container details:
```bash
docker inspect uunatek-api
docker inspect uunatek-sqlserver
```

### Enter containers for debugging:
```bash
# Enter API container
docker exec -it uunatek-api /bin/bash

# Enter SQL Server container
docker exec -it uunatek-sqlserver /bin/bash

# Connect to SQL Server from inside container
docker exec -it uunatek-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C
```

### Remove everything and start fresh:
```bash
# Stop and remove containers and volumes
docker-compose down -v

# Remove all unused Docker data
docker system prune -a

# Start fresh
docker-compose up -d --build
```

### Port already in use:
```bash
# Check what's using port 5000 or 1433
netstat -tuln | grep 5000
netstat -tuln | grep 1433

# Kill the process or change ports in docker-compose.yml
```

## Production Tips

1. **Change the SQL Server password** - Default password is for development only
2. **Use a reverse proxy** (like Nginx) in front of your container for HTTPS
3. **Set up automatic restarts** - already configured with `restart: unless-stopped`
4. **Monitor logs** regularly with `docker logs` or use a logging service
5. **Back up your database** regularly (see Configuration section)
6. **Keep Docker updated** on your Linux server
7. **Limit exposed ports** - Only expose ports you need externally
8. **Use Docker secrets** for sensitive data in production

## Security Notes

- **IMPORTANT:** Change the default SQL Server password in `docker-compose.yml`
- Never commit sensitive passwords to your repository
- The current password is visible in docker-compose.yml - consider using environment variables
- Keep your base images updated (using official Microsoft images)
- SQL Server port 1433 is exposed - restrict access with firewall rules if needed
- Consider using Docker secrets for production deployments

## Architecture

```
Linux Server
├── Docker Network
    ├── uunatek-sqlserver (SQL Server 2022)
    │   └── Port 1433 exposed
    │   └── Data stored in sqlserver_data volume
    │
    └── uunatek-api (.NET 10 API)
        └── Port 5000 exposed
        └── Connects to sqlserver
        └── Runs migrations on startup
```

## Need Help?

If something doesn't work:
1. Check logs: `docker logs uunatek-api` and `docker logs uunatek-sqlserver`
2. Verify Docker is running: `docker ps`
3. Check if ports are in use: `netstat -tuln | grep -E '5000|1433'`
4. Check health status: `docker inspect uunatek-sqlserver | grep Health`
5. Review the Troubleshooting section above
