namespace InEightAIChatbot.Core.Interfaces;

public interface IVectorSearchProvider
{
    /// <summary>
    /// Index document metadata with embedding
    /// </summary>
    Task IndexDocumentAsync(
        int documentId, 
        string text, 
        float[] embedding, 
        Dictionary<string, object> metadata,
        int[] accessibleUserIds,
        CancellationToken ct = default);
    
    /// <summary>
    /// Search documents by embedding similarity (user-filtered)
    /// </summary>
    Task<SearchResult[]> SearchAsync(
        float[] queryEmbedding, 
        int userId, 
        int topK = 10,
        CancellationToken ct = default);
    
    /// <summary>
    /// Delete document from index
    /// </summary>
    Task DeleteDocumentAsync(int documentId, CancellationToken ct = default);
    
    string ProviderName { get; }
}

public record SearchResult(
    int DocumentId,
    string Text,
    Dictionary<string, object> Metadata,
    float Distance
);
