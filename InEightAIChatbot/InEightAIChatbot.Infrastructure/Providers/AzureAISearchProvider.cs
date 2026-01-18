using InEightAIChatbot.Core.Interfaces;

namespace InEightAIChatbot.Infrastructure.Providers;

public class AzureAISearchProvider : IVectorSearchProvider
{
    public string ProviderName => "Azure AI Search";
    
    public Task IndexDocumentAsync(
        int documentId, 
        string text, 
        float[] embedding, 
        Dictionary<string, object> metadata,
        int[] accessibleUserIds,
        CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Phase 2: Install Azure.Search.Documents and use SearchClient.UploadDocumentsAsync");
    }
    
    public Task<SearchResult[]> SearchAsync(
        float[] queryEmbedding, 
        int userId, 
        int topK = 10,
        CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Phase 2: Use SearchClient.SearchAsync with vector search");
    }
    
    public Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        throw new NotImplementedException("Phase 2");
    }
}
