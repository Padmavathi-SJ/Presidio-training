// Domain/Entities/WorkerManagement/Worker.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.YieldReports;

namespace AgriculturePlatform.Domain.Entities.WorkerManagement;

public class Worker
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AdminId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual ICollection<WorkerTask> Tasks { get; set; } = new List<WorkerTask>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
      public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}