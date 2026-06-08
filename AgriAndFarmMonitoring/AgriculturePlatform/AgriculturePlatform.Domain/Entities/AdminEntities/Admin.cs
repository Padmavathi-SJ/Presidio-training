using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class Admin : BaseEntity
{
    public int FarmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
  
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Creator { get; set; }
    public virtual ICollection<Admin> CreatedAdmins { get; set; } = new List<Admin>();
    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();
    public virtual ICollection<CropCycle> CropCycles { get; set; } = new List<CropCycle>();
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<WeatherData> WeatherData { get; set; } = new List<WeatherData>();
    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
    public virtual ICollection<WorkerTask> Tasks { get; set; } = new List<WorkerTask>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    public virtual ICollection<YieldReport> YieldReports { get; set; } = new List<YieldReport>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}