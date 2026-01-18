# PostgreSQL + pgvector Setup for InEight AI Chatbot

## Option 1: Docker (Recommended)

### Start PostgreSQL with pgvector

```powershell
docker run --name postgres-ineight `
  -e POSTGRES_PASSWORD=dev123 `
  -e POSTGRES_DB=ineightchatbot `
  -p 5432:5432 `
  -d ankane/pgvector:latest
```

### Run Setup Script

```powershell
Get-Content Database\PostgreSQL\setup.sql | docker exec -i postgres-ineight psql -U postgres -d ineightchatbot
```

### Verify Installation

```powershell
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot -c "SELECT version();"
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot -c "\dx"
```

## Option 2: Local PostgreSQL Installation

1. Install PostgreSQL 16 from https://www.postgresql.org/download/
2. Install pgvector extension:
   ```powershell
   # Download and install from https://github.com/pgvector/pgvector/releases
   ```
3. Run setup script:
   ```powershell
   psql -U postgres -d ineightchatbot -f Database\PostgreSQL\setup.sql
   ```

## Connection String

Update `appsettings.json` if using different credentials:

```json
"ConnectionStrings": {
  "PostgreSQL": "Host=localhost;Database=ineightchatbot;Username=postgres;Password=dev123"
}
```

## Troubleshooting

### Docker Container Won't Start
```powershell
# Remove existing container
docker rm -f postgres-ineight

# Start fresh
docker run --name postgres-ineight -e POSTGRES_PASSWORD=dev123 -e POSTGRES_DB=ineightchatbot -p 5432:5432 -d ankane/pgvector:latest
```

### Port 5432 Already in Use
```powershell
# Use a different port
docker run --name postgres-ineight -e POSTGRES_PASSWORD=dev123 -e POSTGRES_DB=ineightchatbot -p 5433:5432 -d ankane/pgvector:latest

# Update connection string to use port 5433
```
