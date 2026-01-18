using InEightAIChatbot.Core.Interfaces;

namespace InEightAIChatbot.Infrastructure.Providers;

public class AzureOpenAIProvider : IAIProvider
{
    public string ProviderName => "Azure OpenAI";
    public int EmbeddingDimensions => 1536; // text-embedding-ada-002
    
    public Task<string> GenerateCompletionAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Phase 2: Install Azure.AI.OpenAI NuGet package and implement using OpenAIClient");
    }
    
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Phase 2: Use Azure OpenAI embeddings API");
    }
}
