namespace InEightAIChatbot.Core.Models;

public record ChatRequest(
    string Query,
    int? ProjectId,
    string? SessionId
);

public record ChatResponse(
    string Answer,
    ChatLink[] Links,
    float Confidence,
    string? FallbackKBLink
);

public record ChatLink(
    string Type, // "document" | "filtered_view"
    int? Id,
    string Title,
    string Url
);

public record ChatSession(
    string Id,
    int UserId,
    string Title,
    DateTime CreatedAt
);

public record ChatMessage(
    string Id,
    string SessionId,
    string Role, // "user" | "assistant"
    string Content,
    DateTime Timestamp,
    Dictionary<string, object>? Metadata
);
