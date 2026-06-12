
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using System.Security.Claims;

namespace AgriculturePlatform.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Admin admin);
    string GenerateAccessToken(Worker worker, int farmId);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiryDate();
    bool ValidateToken(string token);
}