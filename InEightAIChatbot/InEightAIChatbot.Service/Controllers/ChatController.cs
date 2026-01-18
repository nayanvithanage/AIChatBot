using InEightAIChatbot.Core.Models;
using InEightAIChatbot.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InEightAIChatbot.Service.Controllers;

[ApiController]
[Route("api/chat")]
// [Authorize] // Temporarily disabled for testing
public class ChatController : ControllerBase
{
    private readonly RAGOrchestrator _ragOrchestrator;
    private readonly ILogger<ChatController> _logger;
    
    public ChatController(RAGOrchestrator ragOrchestrator, ILogger<ChatController> logger)
    {
        _ragOrchestrator = ragOrchestrator;
        _logger = logger;
    }
    
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        
        _logger.LogInformation("Chat request from user {UserId}: {Query}", userId, request.Query);
        
        var response = await _ragOrchestrator.ProcessQueryAsync(
            request.Query, 
            userId, 
            ct);
        
        return Ok(response);
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // For testing without JWT, use user ID 2 (has access to documents)
        return userIdClaim != null ? int.Parse(userIdClaim) : 2;
    }
}
