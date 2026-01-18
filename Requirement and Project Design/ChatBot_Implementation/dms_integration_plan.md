# DMS Integration Plan - Phase 7

## Goal

Integrate the working AI chatbot with the InEight Document Management System (DMS) by:
1. Adding JWT token generation to the DMS
2. Embedding the React chat widget in the DMS layout
3. Re-enabling authentication in the chatbot API
4. Testing end-to-end with real user sessions

---

## Current Status

✅ **Completed:**
- Chatbot API running on http://localhost:5169
- React widget running on http://localhost:5173
- PostgreSQL + pgvector with 3 documents indexed
- Ollama with llama3.2:1b model
- RAG pipeline working end-to-end

⏳ **Pending:**
- JWT token generation in DMS
- Widget embedding in DMS layout
- Re-enable `[Authorize]` attribute
- Production build and deployment

---

## Proposed Changes

### Component 1: JWT Helper in DMS

#### [NEW] InEightDMS.Web/Helpers/JwtHelper.cs

Create a helper class to generate JWT tokens for authenticated users:

```csharp
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace InEightDMS.Web.Helpers
{
    public static class JwtHelper
    {
        // IMPORTANT: This must match the secret in chatbot appsettings.json
        private const string SecretKey = "your-secret-key-min-32-chars-shared-with-dms-change-this-in-production";
        private const string Issuer = "InEightDMS";
        private const string Audience = "InEightChatbot";
        
        public static string GenerateToken(int userId, string userName, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };
            
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // 8-hour token validity
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```

**Dependencies Required:**
Add to `InEightDMS.Web` project via NuGet:
```powershell
Install-Package System.IdentityModel.Tokens.Jwt -Version 7.0.0
```

---

### Component 2: Widget Embedding in Layout

#### [MODIFY] InEightDMS/InEightDMS.Web/Views/Shared/_Layout.cshtml

Add the chat widget to the layout (before closing `</body>` tag):

```cshtml
@using InEightDMS.Web.Helpers
@using Microsoft.AspNet.Identity

<!-- Existing layout content -->

<!-- AI Chatbot Widget -->
@if (Request.IsAuthenticated)
{
    var userId = User.Identity.GetUserId<int>();
    var userName = User.Identity.GetUserName();
    var userRole = User.IsInRole("Admin") ? "Admin" : "User";
    var jwtToken = JwtHelper.GenerateToken(userId, userName, userRole);
    
    <div id="ineight-chatbot-root"></div>
    
    <script>
        // Pass JWT token to widget
        window.INEIGHT_CHATBOT_CONFIG = {
            apiUrl: '@System.Configuration.ConfigurationManager.AppSettings["ChatbotApiUrl"]',
            jwtToken: '@jwtToken'
        };
    </script>
    
    <!-- Load chat widget bundle -->
    <script type="module" src="@Url.Content("~/Scripts/chatbot/chatbot.js")"></script>
}

</body>
</html>
```

**Configuration Required:**
Add to `Web.config`:
```xml
<appSettings>
    <add key="ChatbotApiUrl" value="http://localhost:5169/api" />
    <!-- For production: value="https://your-domain.com/api" -->
</appSettings>
```

---

### Component 3: Build Chat Widget for Production

#### Update chat-widget/src/api.ts

Modify to use configuration from DMS:

```typescript
import { ChatRequest, ChatResponse } from './types';

// Get config from DMS
const config = (window as any).INEIGHT_CHATBOT_CONFIG || {
  apiUrl: 'http://localhost:5169/api',
  jwtToken: null
};

const API_BASE_URL = config.apiUrl;

export class ChatAPI {
  private token: string | null = config.jwtToken;

  setToken(token: string) {
    this.token = token;
  }

  async sendMessage(query: string, projectId?: number): Promise<ChatResponse> {
    const request: ChatRequest = {
      query,
      projectId,
    };

    const response = await fetch(`${API_BASE_URL}/chat/message`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(this.token && { 'Authorization': `Bearer ${this.token}` }),
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.statusText}`);
    }

    return response.json();
  }
}

export const chatAPI = new ChatAPI();
```

#### Build Widget for Production

```powershell
cd InEightAIChatbot/chat-widget

# Build for production
npm run build

# Output will be in dist/ folder
# Copy dist/assets/* to InEightDMS.Web/Scripts/chatbot/
```

---

### Component 4: Re-enable Authentication in Chatbot API

#### [MODIFY] InEightAIChatbot.Service/Controllers/ChatController.cs

Remove testing code and re-enable authentication:

```csharp
using InEightAIChatbot.Core.Models;
using InEightAIChatbot.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InEightAIChatbot.Service.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize] // ← Re-enable this
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
        return int.Parse(userIdClaim!); // Remove fallback to test user ID
    }
}
```

---

### Component 5: Update CORS for Production

#### [MODIFY] InEightAIChatbot.Service/Program.cs

Update CORS to allow DMS domain:

```csharp
// Configure CORS for DMS integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DMS", policy =>
    {
        policy.WithOrigins(
                // Development
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "https://localhost:5173",
                // DMS (adjust port as needed)
                "http://localhost:44300",
                "https://localhost:44300",
                // Production (add your domain)
                "https://your-dms-domain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

## Verification Plan

### Step 1: Test JWT Generation

```csharp
// Add a test endpoint in DMS HomeController
public ActionResult TestJwt()
{
    var userId = User.Identity.GetUserId<int>();
    var userName = User.Identity.GetUserName();
    var token = JwtHelper.GenerateToken(userId, userName, "User");
    
    return Content($"JWT Token: {token}");
}
```

Visit `/Home/TestJwt` and verify token is generated.

### Step 2: Build and Deploy Widget

```powershell
# 1. Build widget
cd InEightAIChatbot/chat-widget
npm run build

# 2. Copy to DMS
Copy-Item -Path "dist/assets/*" -Destination "../../InEightDMS/InEightDMS.Web/Scripts/chatbot/" -Recurse -Force

# 3. Verify files copied
ls ../../InEightDMS/InEightDMS.Web/Scripts/chatbot/
```

### Step 3: Test Integration

1. **Start Chatbot API**:
   ```powershell
   cd InEightAIChatbot
   dotnet run --project InEightAIChatbot.Service
   ```

2. **Start DMS**:
   - Run InEightDMS.Web from Visual Studio
   - Login with a test user

3. **Verify Widget Appears**:
   - Check bottom-right corner for chat widget
   - Open browser console (F12) - should see no errors
   - Check `window.INEIGHT_CHATBOT_CONFIG` has JWT token

4. **Test Chat Functionality**:
   - Ask: "Show me recent documents"
   - Verify: Response includes documents user has access to
   - Check: Document links work and navigate to correct pages

### Step 4: Test User Access Control

1. Login as **User A** (has access to Project 1)
2. Ask about documents in Project 1 → Should work
3. Login as **User B** (no access to Project 1)
4. Ask same question → Should return different results or "no access"

---

## Deployment Checklist

### Development Environment
- [ ] JWT secret configured in both DMS and Chatbot
- [ ] Widget built and copied to DMS Scripts folder
- [ ] CORS allows DMS localhost URL
- [ ] Authentication re-enabled in ChatController
- [ ] Test with multiple users

### Production Environment
- [ ] Change JWT secret to strong random key (min 32 chars)
- [ ] Update `ChatbotApiUrl` in Web.config to production URL
- [ ] Update CORS to allow production DMS domain
- [ ] Deploy chatbot API to production server
- [ ] Ensure PostgreSQL and Ollama are accessible
- [ ] Test end-to-end in production

---

## Security Considerations

> [!WARNING]
> **JWT Secret Management**
> 
> - The JWT secret MUST be the same in both DMS and Chatbot
> - Use a strong random key (min 32 characters)
> - Store in secure configuration (Azure Key Vault for production)
> - Never commit secrets to source control

> [!IMPORTANT]
> **User Access Control**
> 
> - Chatbot respects DMS project assignments
> - Users only see documents they have access to
> - JWT tokens expire after every chat session when closed chat window or after 2 hours of inactivity (configurable in settings)
> - Re-authentication required after expiry

---

## Rollback Plan

If integration causes issues:

1. **Quick Disable**: Comment out widget section in `_Layout.cshtml`
2. **Full Rollback**: Remove JWT helper and widget files
3. **Chatbot Still Works**: Standalone at http://localhost:5173 for testing

---

## Next Steps After Integration

1. **Monitor Usage**: Add analytics to track chat queries
2. **Gather Feedback**: Collect user feedback on chatbot responses
3. **Optimize**: Fine-tune prompts based on common queries
4. **Expand**: Add more document types (RFIs, Transmittals)
5. **Upgrade**: Consider Azure OpenAI for better performance

---

## Files to Create/Modify

### New Files:
1. `InEightDMS.Web/Helpers/JwtHelper.cs`
2. `InEightDMS.Web/Scripts/chatbot/` (widget build output)

### Modified Files:
1. `InEightDMS.Web/Views/Shared/_Layout.cshtml`
2. `InEightDMS.Web/Web.config`
3. `InEightAIChatbot/chat-widget/src/api.ts`
4. `InEightAIChatbot.Service/Controllers/ChatController.cs`
5. `InEightAIChatbot.Service/Program.cs`

---

## Estimated Time

- JWT Helper Creation: 15 minutes
- Widget Build & Integration: 30 minutes
- Testing & Debugging: 1-2 hours
- **Total**: 2-3 hours

---

## Success Criteria

✅ Users can see chat widget when logged into DMS  
✅ Chat widget authenticates using JWT from DMS  
✅ Users can ask questions and get relevant responses  
✅ Document links navigate to correct DMS pages  
✅ User access control works correctly  
✅ No console errors in browser  
✅ API responds within 3-5 seconds
