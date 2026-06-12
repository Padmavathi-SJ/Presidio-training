// Domain/Entities/AdminEntities/RefreshToken.cs
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class RefreshToken : BaseEntity
{
    public int AdminId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public string? RevokedByIp { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Navigation property
    public virtual Admin? Admin { get; set; }
}