using InEightAIChatbot.Core.Interfaces;
using InEightAIChatbot.Core.Models;
using Microsoft.Extensions.Logging;

namespace InEightAIChatbot.Service.Services;

public class RAGOrchestrator
{
    private readonly IAIProvider _aiProvider;
    private readonly IVectorSearchProvider _vectorSearch;
    private readonly ILogger<RAGOrchestrator> _logger;
    
    public RAGOrchestrator(
        IAIProvider aiProvider,
        IVectorSearchProvider vectorSearch,
        ILogger<RAGOrchestrator> logger)
    {
        _aiProvider = aiProvider;
        _vectorSearch = vectorSearch;
        _logger = logger;
    }
    
    public async Task<ChatResponse> ProcessQueryAsync(
        string userQuery, 
        int userId, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Processing query for user {UserId}: {Query}", userId, userQuery);
        
        try
        {
            // 1. Generate query embedding
            var queryEmbedding = await _aiProvider.GenerateEmbeddingAsync(userQuery, ct);
            
            // 2. Semantic search (user-filtered)
            var results = await _vectorSearch.SearchAsync(queryEmbedding, userId, topK: 10, ct);
            
            if (results.Length == 0)
            {
                return new ChatResponse(
                    Answer: "I couldn't find any documents matching your query. You may not have access to relevant documents, or they may not exist.",
                    Links: Array.Empty<ChatLink>(),
                    Confidence: 0,
                    FallbackKBLink: "https://learn.ineight.com/Document_Enhanced/Content/Categories/Home-Page.htm"
                );
            }
            
            // 3. Build context from top results
            var context = string.Join("\n\n", results.Select(r => r.Text));
            
            // 4. Generate answer using LLM
            var systemPrompt = @"You are a helpful document management assistant for InEight Document system.
Answer questions based ONLY on the provided context. 
Be concise and specific. 
If you reference a document, mention its name.
If the context doesn't contain enough information, say so.";
            
            var userPrompt = $"Context:\n{context}\n\nQuestion: {userQuery}";
            
            var answer = await _aiProvider.GenerateCompletionAsync(systemPrompt, userPrompt, ct);
            
            // 5. Calculate confidence based on semantic distance
            var avgDistance = results.Average(r => r.Distance);
            var confidence = 1.0f - Math.Min(avgDistance, 1.0f);
            
            // 6. Build response links
            var links = results
                .Take(5)
                .Select(r => new ChatLink(
                    Type: "document",
                    Id: r.DocumentId,
                    Title: r.Metadata["name"].ToString()!,
                    Url: $"/Documents/Details/{r.DocumentId}?projectId={r.Metadata["projectId"]}"
                ))
                .ToArray();
            
            return new ChatResponse(
                Answer: answer,
                Links: links,
                Confidence: confidence,
                FallbackKBLink: confidence < 0.7f 
                    ? "https://learn.ineight.com/Document_Enhanced/Content/Categories/Home-Page.htm" 
                    : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query for user {UserId}", userId);
            return new ChatResponse(
                Answer: "I encountered an error processing your request. Please try again.",
                Links: Array.Empty<ChatLink>(),
                Confidence: 0,
                FallbackKBLink: "https://learn.ineight.com/Document_Enhanced/Content/Categories/Home-Page.htm"
            );
        }
    }
}
