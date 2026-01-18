using InEightAIChatbot.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InEightAIChatbot.Service.Services;

public class MetadataSyncService : BackgroundService
{
    private readonly IAIProvider _aiProvider;
    private readonly IVectorSearchProvider _vectorSearch;
    private readonly ILogger<MetadataSyncService> _logger;
    private readonly string _dmsConnectionString;
    
    public MetadataSyncService(
        IAIProvider aiProvider,
        IVectorSearchProvider vectorSearch,
        ILogger<MetadataSyncService> logger,
        IConfiguration configuration)
    {
        _aiProvider = aiProvider;
        _vectorSearch = vectorSearch;
        _logger = logger;
        _dmsConnectionString = configuration.GetConnectionString("DMSDatabase")!;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("MetadataSyncService started using {Provider} and {VectorStore}", 
            _aiProvider.ProviderName, _vectorSearch.ProviderName);
        
        // Wait a bit before starting initial sync
        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        
        // Initial sync
        await SyncAllDocumentsAsync(ct);
        
        // Periodic sync every 4 hours
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(4), ct);
            await SyncAllDocumentsAsync(ct);
        }
    }
    
    private async Task SyncAllDocumentsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting document metadata sync...");
        
        try
        {
            await using var connection = new SqlConnection(_dmsConnectionString);
            await connection.OpenAsync(ct);
            
            // Query documents with all metadata from DMS database
            var query = @"
                SELECT 
                    d.Id, d.Name, d.Description, d.Type, d.Category, d.Tags,
                    d.Status, d.TransmittalNumber, d.Version, d.RevisionNumber,
                    d.UploadedAt, d.ProjectId,
                    u.Name AS UploadedByName,
                    p.Name AS ProjectName,
                    d.ApprovalStatus
                FROM Documents d
                INNER JOIN AspNetUsers u ON d.UploadedById = u.Id
                INNER JOIN Projects p ON d.ProjectId = p.Id
                WHERE d.Status != 6"; // Exclude archived
            
            await using var cmd = new SqlCommand(query, connection);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            
            var documentsToSync = new List<(int Id, string Text, Dictionary<string, object> Metadata, int ProjectId)>();
            
            while (await reader.ReadAsync(ct))
            {
                var documentId = reader.GetInt32(0);
                var text = FormatDocumentMetadata(reader);
                var metadata = ExtractMetadata(reader);
                var projectId = reader.GetInt32(11);
                
                documentsToSync.Add((documentId, text, metadata, projectId));
            }
            
            await reader.CloseAsync();
            
            // Now process each document
            int synced = 0;
            foreach (var doc in documentsToSync)
            {
                var accessibleUserIds = await GetAccessibleUserIdsAsync(doc.ProjectId, ct);
                
                // Generate embedding
                var embedding = await _aiProvider.GenerateEmbeddingAsync(doc.Text, ct);
                
                // Index
                await _vectorSearch.IndexDocumentAsync(
                    doc.Id, doc.Text, embedding, doc.Metadata, accessibleUserIds, ct);
                
                synced++;
            }
            
            _logger.LogInformation("Synced {Count} documents", synced);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during metadata sync");
        }
    }
    
    private string FormatDocumentMetadata(SqlDataReader reader)
    {
        return $"Document: {reader.GetString(1)}\n" +
               $"Description: {(reader.IsDBNull(2) ? "No description" : reader.GetString(2))}\n" +
               $"Type: {(reader.IsDBNull(3) ? "Unknown" : reader.GetString(3))}\n" +
               $"Category: {(reader.IsDBNull(4) ? "Uncategorized" : reader.GetString(4))}\n" +
               $"Status: {reader.GetInt32(6)}\n" +
               $"Project: {reader.GetString(13)}\n" +
               $"Uploaded By: {reader.GetString(12)} on {reader.GetDateTime(10):yyyy-MM-dd}\n" +
               $"Transmittal: {(reader.IsDBNull(7) ? "None" : reader.GetString(7))}\n" +
               $"Version: {reader.GetInt32(8)}, Revision: {reader.GetInt32(9)}";
    }
    
    private Dictionary<string, object> ExtractMetadata(SqlDataReader reader)
    {
        return new Dictionary<string, object>
        {
            ["documentId"] = reader.GetInt32(0),
            ["name"] = reader.GetString(1),
            ["type"] = reader.GetString(3),
            ["projectId"] = reader.GetInt32(11),
            ["projectName"] = reader.GetString(13),
            ["status"] = reader.GetInt32(6)
        };
    }
    
    private async Task<int[]> GetAccessibleUserIdsAsync(int projectId, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_dmsConnectionString);
        await connection.OpenAsync(ct);
        
        // Get project manager + all project users + admins
        var query = @"
            SELECT DISTINCT u.Id
            FROM AspNetUsers u
            LEFT JOIN Projects p ON p.ManagerId = u.Id
            LEFT JOIN ProjectUsers pu ON pu.UserId = u.Id
            WHERE p.Id = @ProjectId OR pu.ProjectId = @ProjectId OR u.Role = 1"; // Include admins
        
        await using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@ProjectId", projectId);
        
        var userIds = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            userIds.Add(reader.GetInt32(0));
        }
        
        return userIds.ToArray();
    }
}
