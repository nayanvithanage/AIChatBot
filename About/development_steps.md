# InEight AI Chatbot - Development Steps

This document contains all the scripts, commands, and steps used to implement the InEight AI Chatbot from start to finish.

---

## Phase 1: Project Setup

### 1.1 Create Solution Structure

```powershell
# Navigate to project root
cd d:\Code\ineight\projects\InEightDocumentAISuite

# Create solution
dotnet new sln -n InEightAIChatbot

# Create projects
dotnet new webapi -n InEightAIChatbot.Service -f net8.0
dotnet new classlib -n InEightAIChatbot.Core -f net8.0
dotnet new classlib -n InEightAIChatbot.Infrastructure -f net8.0

# Add to solution
dotnet sln InEightAIChatbot.sln add InEightAIChatbot.Service
dotnet sln InEightAIChatbot.sln add InEightAIChatbot.Core
dotnet sln InEightAIChatbot.sln add InEightAIChatbot.Infrastructure

# Add project references
dotnet add InEightAIChatbot.Service reference InEightAIChatbot.Core
dotnet add InEightAIChatbot.Service reference InEightAIChatbot.Infrastructure
dotnet add InEightAIChatbot.Infrastructure reference InEightAIChatbot.Core
```

### 1.2 Install NuGet Packages

```powershell
# Navigate to Service project
cd InEightAIChatbot/InEightAIChatbot.Service

# Add required packages
dotnet add package Npgsql --version 8.0.0
dotnet add package Pgvector --version 0.2.0
dotnet add package Microsoft.Data.SqlClient --version 5.1.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

---

## Phase 2: Infrastructure Setup

### 2.1 Install Docker Desktop

1. Download Docker Desktop for Windows
2. Install and restart computer
3. Start Docker Desktop
4. Verify installation:

```powershell
docker --version
```

### 2.2 Setup PostgreSQL with pgvector

```powershell
# Pull PostgreSQL image with pgvector
docker pull pgvector/pgvector:pg16

# Run PostgreSQL container on port 5433 (to avoid conflict)
docker run -d `
  --name postgres-ineight `
  -e POSTGRES_PASSWORD=dev123 `
  -e POSTGRES_DB=ineightchatbot `
  -p 5433:5432 `
  pgvector/pgvector:pg16

# Verify container is running
docker ps

# Access PostgreSQL shell
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot
```

### 2.3 Create Database Schema

```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create document embeddings table
CREATE TABLE document_embeddings (
    document_id INTEGER PRIMARY KEY,
    chunk_text TEXT NOT NULL,
    embedding vector(768) NOT NULL,
    metadata JSONB NOT NULL,
    user_access_list INTEGER[] NOT NULL,
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create index for vector similarity search
CREATE INDEX ON document_embeddings 
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- Create chat sessions table
CREATE TABLE chat_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id INTEGER NOT NULL,
    title TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create chat messages table
CREATE TABLE chat_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES chat_sessions(id),
    role TEXT NOT NULL CHECK (role IN ('user', 'assistant')),
    content TEXT NOT NULL,
    metadata JSONB,
    timestamp TIMESTAMP DEFAULT NOW()
);

-- Create indexes
CREATE INDEX idx_chat_messages_session ON chat_messages(session_id);
CREATE INDEX idx_chat_sessions_user ON chat_sessions(user_id);

-- Verify tables
\dt

-- Verify vector extension
SELECT * FROM pg_extension WHERE extname = 'vector';

-- Exit psql
\q
```

### 2.4 Install Ollama

```powershell
# Download Ollama from https://ollama.ai/download
# Install and restart terminal

# Verify installation
ollama --version

# Pull required models
ollama pull llama3.2:1b
ollama pull nomic-embed-text

# Verify models
ollama list

# Test embedding generation
ollama run nomic-embed-text "test"

# Test text generation
ollama run llama3.2:1b "Hello, just say OK"
```

---

## Phase 3: Core Implementation

### 3.1 Build and Run API

```powershell
# Navigate to solution root
cd d:\Code\ineight\projects\InEightDocumentAISuite\InEightAIChatbot

# Build solution
dotnet build

# Run API
dotnet run --project InEightAIChatbot.Service

# API will start on http://localhost:5169
# Swagger UI: http://localhost:5169/swagger
```

### 3.2 Test Document Sync

```powershell
# API automatically syncs documents on startup
# Check logs for:
# "Starting document metadata sync..."
# "Synced X documents"

# Verify in PostgreSQL
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot

# Check synced documents
SELECT document_id, metadata->>'name' as name, 
       cardinality(user_access_list) as num_users
FROM document_embeddings;

# Exit
\q
```

### 3.3 Test Chat Endpoint

```powershell
# Using PowerShell to test API
$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    query = "Show me recent documents"
    projectId = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5169/api/chat/message" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

---

## Phase 4: React Chat Widget

### 4.1 Create React Project

```powershell
# Navigate to chatbot folder
cd d:\Code\ineight\projects\InEightDocumentAISuite\InEightAIChatbot

# Create Vite + React + TypeScript project
npm create vite@latest chat-widget -- --template react-ts

# Navigate to widget folder
cd chat-widget

# Install dependencies
npm install

# Start development server
npm run dev

# Widget runs on http://localhost:5173
```

### 4.2 Build for Production

```powershell
# Build production bundle
npm run build

# Output will be in dist/ folder
# Files: index.html, assets/index-[hash].js, assets/index-[hash].css
```

### 4.3 Copy to DMS

```powershell
# Create chatbot folder in DMS
New-Item -ItemType Directory -Path "d:\Code\ineight\projects\InEightDocumentAISuite\InEightDMS\InEightDMS.Web\Scripts\chatbot" -Force

# Copy build output
Copy-Item -Path "d:\Code\ineight\projects\InEightDocumentAISuite\InEightAIChatbot\chat-widget\dist\*" `
    -Destination "d:\Code\ineight\projects\InEightDocumentAISuite\InEightDMS\InEightDMS.Web\Scripts\chatbot\" `
    -Recurse -Force

# Verify files copied
ls d:\Code\ineight\projects\InEightDocumentAISuite\InEightDMS\InEightDMS.Web\Scripts\chatbot\
```

---

## Phase 5: DMS Integration

### 5.1 Install JWT Package in DMS

**In Visual Studio:**
1. Open Package Manager Console
2. Select `InEightDMS.Web` project
3. Run:

```powershell
Install-Package System.IdentityModel.Tokens.Jwt -Version 7.0.0
```

### 5.2 Build DMS

**In Visual Studio:**
1. Build → Rebuild Solution (Ctrl+Shift+B)
2. Run DMS (F5)
3. DMS will start on http://localhost:52291

### 5.3 Update API CORS

```powershell
# Stop API (Ctrl+C)
# Edit Program.cs to add DMS port
# Restart API
dotnet run --project InEightAIChatbot.Service
```

---

## Phase 6: Testing & Verification

### 6.1 Database Verification

```sql
-- Connect to PostgreSQL
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot

-- Check document count
SELECT COUNT(*) FROM document_embeddings;

-- Check user access
SELECT document_id, user_access_list FROM document_embeddings;

-- Check vector dimensions
SELECT document_id, vector_dims(embedding) FROM document_embeddings;

-- Test vector search
SELECT document_id, 
       embedding <=> '[0.1, 0.2, ...]'::vector as distance
FROM document_embeddings
WHERE 2 = ANY(user_access_list)
ORDER BY distance
LIMIT 5;
```

### 6.2 API Health Check

```powershell
# Check API is running
Invoke-RestMethod -Uri "http://localhost:5169/swagger/index.html"

# Check Ollama connection
Invoke-RestMethod -Uri "http://localhost:11434/api/tags"

# Check PostgreSQL connection
docker exec postgres-ineight pg_isready -U postgres
```

### 6.3 End-to-End Test

1. Start all services:
   - PostgreSQL: `docker start postgres-ineight`
   - Ollama: `ollama serve` (runs in background)
   - API: `dotnet run --project InEightAIChatbot.Service`
   - DMS: Run from Visual Studio (F5)

2. Login to DMS
3. Look for floating chat button (bottom-right)
4. Click button to open chat
5. Type: "Show me recent documents"
6. Verify response with document links

---

## Phase 7: Troubleshooting Commands

### 7.1 Docker Commands

```powershell
# List running containers
docker ps

# List all containers
docker ps -a

# Start container
docker start postgres-ineight

# Stop container
docker stop postgres-ineight

# Remove container
docker rm postgres-ineight

# View logs
docker logs postgres-ineight

# Access shell
docker exec -it postgres-ineight bash
```

### 7.2 PostgreSQL Commands

```sql
-- List databases
\l

-- Connect to database
\c ineightchatbot

-- List tables
\dt

-- Describe table
\d document_embeddings

-- Check table size
SELECT pg_size_pretty(pg_total_relation_size('document_embeddings'));

-- Delete all data
TRUNCATE TABLE document_embeddings CASCADE;

-- Drop and recreate
DROP TABLE IF EXISTS document_embeddings CASCADE;
```

### 7.3 Ollama Commands

```powershell
# List models
ollama list

# Pull model
ollama pull llama3.2:1b

# Remove model
ollama rm llama3.2:1b

# Show model info
ollama show llama3.2:1b

# Test model
ollama run llama3.2:1b "test"

# Stop Ollama service
# (Close terminal or Ctrl+C if running in foreground)
```

### 7.4 .NET Commands

```powershell
# Clean build
dotnet clean

# Restore packages
dotnet restore

# Build
dotnet build

# Run with specific configuration
dotnet run --project InEightAIChatbot.Service --configuration Release

# Watch mode (auto-rebuild on changes)
dotnet watch run --project InEightAIChatbot.Service

# List project references
dotnet list reference
```

### 7.5 npm Commands

```powershell
# Install dependencies
npm install

# Run dev server
npm run dev

# Build production
npm run build

# Clean node_modules
rm -r node_modules
npm install

# Update packages
npm update

# Check for outdated packages
npm outdated
```

---

## Phase 8: Configuration Updates

### 8.1 Update appsettings.json

```json
{
  "AI": {
    "Provider": "Ollama",
    "VectorStore": "PgVector",
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "Model": "llama3.2:1b",
      "EmbeddingModel": "nomic-embed-text"
    }
  },
  "ConnectionStrings": {
    "DMSDatabase": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=InEightDMS;Integrated Security=True",
    "PostgreSQL": "Host=localhost;Port=5433;Database=ineightchatbot;Username=postgres;Password=dev123;Pooling=false"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars-shared-with-dms-change-this-in-production",
    "Issuer": "InEightDMS",
    "Audience": "InEightChatbot"
  }
}
```

### 8.2 Update Web.config (DMS)

```xml
<appSettings>
  <add key="ChatbotApiUrl" value="http://localhost:5169/api" />
</appSettings>
```

---

## Phase 9: Deployment Checklist

### 9.1 Pre-Deployment

```powershell
# 1. Build all projects
dotnet build --configuration Release

# 2. Run tests (if any)
dotnet test

# 3. Build React widget
cd chat-widget
npm run build

# 4. Copy widget to DMS
Copy-Item -Path "dist\*" -Destination "..\..\..\InEightDMS\InEightDMS.Web\Scripts\chatbot\" -Recurse -Force

# 5. Verify database
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot -c "SELECT COUNT(*) FROM document_embeddings;"

# 6. Test Ollama
ollama list
```

### 9.2 Production Deployment (Future)

```powershell
# Deploy to Azure App Service
az webapp up --name ineight-chatbot-api --resource-group ineight-rg

# Deploy PostgreSQL to Azure
az postgres flexible-server create --name ineight-postgres --resource-group ineight-rg

# Configure Azure OpenAI
az cognitiveservices account create --name ineight-openai --resource-group ineight-rg --kind OpenAI

# Update appsettings.json with Azure endpoints
```

---

## Phase 10: Maintenance Scripts

### 10.1 Daily Health Check

```powershell
# Check all services
$services = @(
    @{Name="PostgreSQL"; Command="docker ps --filter name=postgres-ineight"},
    @{Name="Ollama"; Command="ollama list"},
    @{Name="API"; Command="Invoke-RestMethod -Uri http://localhost:5169/swagger/index.html"}
)

foreach ($service in $services) {
    Write-Host "Checking $($service.Name)..."
    Invoke-Expression $service.Command
}
```

### 10.2 Database Backup

```powershell
# Backup PostgreSQL database
docker exec postgres-ineight pg_dump -U postgres ineightchatbot > backup_$(Get-Date -Format 'yyyyMMdd').sql

# Restore from backup
cat backup_20260119.sql | docker exec -i postgres-ineight psql -U postgres -d ineightchatbot
```

### 10.3 Clear Cache

```powershell
# Clear browser cache
# Ctrl+Shift+Delete in browser

# Clear .NET build cache
dotnet clean
rm -r bin, obj -Recurse -Force

# Clear npm cache
npm cache clean --force
```

### 10.4 Resync Documents

```powershell
# Truncate embeddings table
docker exec -it postgres-ineight psql -U postgres -d ineightchatbot -c "TRUNCATE TABLE document_embeddings;"

# Restart API (will trigger automatic sync)
# Ctrl+C to stop
dotnet run --project InEightAIChatbot.Service
```

---

## Summary of Key Commands

### Start All Services

```powershell
# 1. Start PostgreSQL
docker start postgres-ineight

# 2. Start Ollama (runs automatically in background)
# If not running: ollama serve

# 3. Start API
cd d:\Code\ineight\projects\InEightDocumentAISuite\InEightAIChatbot
dotnet run --project InEightAIChatbot.Service

# 4. Start DMS (in Visual Studio)
# Press F5
```

### Stop All Services

```powershell
# 1. Stop API
# Ctrl+C in terminal

# 2. Stop DMS
# Stop debugging in Visual Studio

# 3. Stop PostgreSQL
docker stop postgres-ineight

# 4. Ollama continues running (optional to stop)
```

### Quick Test

```powershell
# Test API
Invoke-RestMethod -Uri "http://localhost:5169/swagger/index.html"

# Test PostgreSQL
docker exec postgres-ineight pg_isready -U postgres

# Test Ollama
ollama list

# Test DMS
# Navigate to http://localhost:52291 in browser
```

---

## Appendix: Common Issues & Solutions

### Issue: Port Already in Use

```powershell
# Find process using port
netstat -ano | findstr :5433

# Kill process
taskkill /PID <process_id> /F
```

### Issue: Docker Container Won't Start

```powershell
# Remove and recreate container
docker rm -f postgres-ineight
docker run -d --name postgres-ineight -e POSTGRES_PASSWORD=dev123 -e POSTGRES_DB=ineightchatbot -p 5433:5432 pgvector/pgvector:pg16
```

### Issue: Ollama Model Not Found

```powershell
# Re-pull model
ollama pull llama3.2:1b
ollama pull nomic-embed-text
```

### Issue: Build Errors

```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Issue: Widget Not Loading

```powershell
# Rebuild widget
cd chat-widget
npm run build

# Copy to DMS
Copy-Item -Path "dist\*" -Destination "..\..\..\InEightDMS\InEightDMS.Web\Scripts\chatbot\" -Recurse -Force

# Rebuild DMS in Visual Studio
# Ctrl+Shift+B
```

---

**End of Development Steps**
