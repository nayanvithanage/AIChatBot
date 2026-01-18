using InEightAIChatbot.Core.Interfaces;
using Npgsql;
using Pgvector;
using System.Text.Json;

namespace InEightAIChatbot.Infrastructure.Providers;

public class PgVectorProvider : IVectorSearchProvider
{
    private readonly NpgsqlDataSource _dataSource;
    
    public string ProviderName => "PostgreSQL + pgvector";
    
    public PgVectorProvider(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    
    public async Task IndexDocumentAsync(
        int documentId, 
        string text, 
        float[] embedding, 
        Dictionary<string, object> metadata,
        int[] accessibleUserIds,
        CancellationToken ct = default)
    {
        await using var cmd = _dataSource.CreateCommand(@"
            INSERT INTO document_embeddings 
            (document_id, chunk_text, embedding, metadata, user_access_list, updated_at)
            VALUES ($1, $2, $3, $4::jsonb, $5, NOW())
            ON CONFLICT (document_id) 
            DO UPDATE SET 
                chunk_text = EXCLUDED.chunk_text,
                embedding = EXCLUDED.embedding,
                metadata = EXCLUDED.metadata,
                user_access_list = EXCLUDED.user_access_list,
                updated_at = NOW()");
        
        cmd.Parameters.AddWithValue(documentId);
        cmd.Parameters.AddWithValue(text);
        cmd.Parameters.AddWithValue(new Vector(embedding));
        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(metadata));
        cmd.Parameters.AddWithValue(accessibleUserIds);
        
        await cmd.ExecuteNonQueryAsync(ct);
    }
    
    public async Task<SearchResult[]> SearchAsync(
        float[] queryEmbedding, 
        int userId, 
        int topK = 10,
        CancellationToken ct = default)
    {
        await using var cmd = _dataSource.CreateCommand(@"
            SELECT document_id, chunk_text, metadata,
                   embedding <=> $1 AS distance
            FROM document_embeddings
            WHERE $2 = ANY(user_access_list)
            ORDER BY distance
            LIMIT $3");
        
        cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(topK);
        
        var results = new List<SearchResult>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        
        while (await reader.ReadAsync(ct))
        {
            results.Add(new SearchResult(
                DocumentId: reader.GetInt32(0),
                Text: reader.GetString(1),
                Metadata: JsonSerializer.Deserialize<Dictionary<string, object>>(
                    reader.GetString(2))!,
                Distance: reader.GetFloat(3)
            ));
        }
        
        return results.ToArray();
    }
    
    public async Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        await using var cmd = _dataSource.CreateCommand(
            "DELETE FROM document_embeddings WHERE document_id = $1");
        cmd.Parameters.AddWithValue(documentId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
