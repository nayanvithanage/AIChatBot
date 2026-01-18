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
