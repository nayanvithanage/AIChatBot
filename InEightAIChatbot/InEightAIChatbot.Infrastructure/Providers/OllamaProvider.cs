using InEightAIChatbot.Core.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace InEightAIChatbot.Infrastructure.Providers;

public class OllamaProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;
    
    public string ProviderName => "Ollama";
    public int EmbeddingDimensions => 768; // nomic-embed-text actual dimensions
    
    public OllamaProvider(HttpClient httpClient, IOptions<OllamaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }
    
    public async Task<string> GenerateCompletionAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken ct = default)
    {
        var request = new
        {
            model = _settings.Model,
            prompt = $"{systemPrompt}\n\n{userPrompt}",
            stream = false,
            options = new
            {
                temperature = 0.7,
                top_p = 0.9
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{_settings.Endpoint}/api/generate", 
            request, ct);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(ct);
        return result!.Response;
    }
    
    public async Task<float[]> GenerateEmbeddingAsync(
        string text, 
        CancellationToken ct = default)
    {
        var request = new
        {
            model = _settings.EmbeddingModel,
            prompt = text
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{_settings.Endpoint}/api/embeddings", 
            request, ct);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(ct);
        return result!.Embedding;
    }
}

public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
}

internal record OllamaResponse(string Response);
internal record OllamaEmbeddingResponse(float[] Embedding);
