using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class RefreshToken : BaseEntity
{
    public int? AdminId { get; set; }
    public int? WorkerId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public string? RevokedByIp { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Navigation properties
    public virtual Admin? Admin { get; set; }
    public virtual Worker? Worker { get; set; }
}