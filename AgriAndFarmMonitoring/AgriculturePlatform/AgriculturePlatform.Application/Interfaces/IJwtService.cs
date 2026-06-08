// AgriculturePlatform.Application/Interfaces/IJwtService.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Admin admin);
    DateTime GetExpiryDate();
}