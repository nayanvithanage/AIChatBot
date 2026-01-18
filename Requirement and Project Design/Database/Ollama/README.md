# Ollama Setup for InEight AI Chatbot

## Installation

### Windows

1. Download Ollama from https://ollama.com/download
2. Run the installer
3. Ollama will start automatically and run in the background

## Pull Required Models

Open PowerShell and run:

```powershell
# Pull Llama 3 for text generation (about 4.7GB)
ollama pull llama3

# Pull nomic-embed-text for embeddings (about 274MB)
ollama pull nomic-embed-text
```

## Verify Installation

```powershell
# Test Llama 3
ollama run llama3 "Hello, how are you?"

# Test embeddings
curl http://localhost:11434/api/embeddings -d '{\"model\":\"nomic-embed-text\",\"prompt\":\"Test\"}'
```

## Configuration

Ollama runs on `http://localhost:11434` by default. This is already configured in `appsettings.json`:

```json
"Ollama": {
  "Endpoint": "http://localhost:11434",
  "Model": "llama3",
  "EmbeddingModel": "nomic-embed-text"
}
```

## Performance Notes

- **CPU Mode**: Llama 3 will run on CPU, expect 2-5 second response times
- **GPU Mode**: If you have an NVIDIA GPU, Ollama will automatically use it for faster responses (0.5-2 seconds)
- **Memory**: Llama 3 requires about 8GB RAM when loaded

## Troubleshooting

### Ollama Not Running
```powershell
# Check if Ollama is running
curl http://localhost:11434/api/tags

# If not, start Ollama from Start Menu
```

### Model Not Found
```powershell
# List installed models
ollama list

# Pull missing model
ollama pull llama3
ollama pull nomic-embed-text
```

### Slow Performance
- First request is always slower (model loading)
- Subsequent requests are faster
- Consider using GPU if available
- Reduce model size: use `llama3:8b` instead of default

## Alternative Models

If you want to try different models:

```powershell
# Smaller, faster model
ollama pull llama3:8b

# Larger, more accurate model  
ollama pull llama3:70b
```

Update `appsettings.json` accordingly:
```json
"Model": "llama3:8b"
```
