using Assignment3.Models;
using System.Security.Claims;

namespace Assignment3.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
        long GetUserIdFromToken(string token);
    }
}
