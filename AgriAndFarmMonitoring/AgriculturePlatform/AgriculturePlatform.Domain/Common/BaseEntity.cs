// AgriculturePlatform.Domain/Common/BaseEntity.cs
namespace AgriculturePlatform.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Soft Delete properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Audit properties
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}