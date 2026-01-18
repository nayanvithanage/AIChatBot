namespace InEightAIChatbot.Core.Interfaces;

public interface IAIProvider
{
    /// <summary>
    /// Generate text completion from prompts
    /// </summary>
    Task<string> GenerateCompletionAsync(
        string systemPrompt, 
        string userPrompt, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Generate embedding vector for text
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(
        string text, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Provider name for logging
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Embedding vector dimensions
    /// </summary>
    int EmbeddingDimensions { get; }
}
