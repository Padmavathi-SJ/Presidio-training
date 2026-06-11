using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.WorkerManagement;

public class Worker : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }  // MANAGER, SUPERVISOR, OPERATOR, LABOR
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Additional fields for better worker management
    public string? ProfilePictureUrl { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Department { get; set; }  // e.g., Harvesting, Quality Control, Irrigation, etc.
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual ICollection<WorkerTask> Tasks { get; set; } = new List<WorkerTask>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<WorkerFieldAssignment> AssignedFields { get; set; } = new List<WorkerFieldAssignment>();
}