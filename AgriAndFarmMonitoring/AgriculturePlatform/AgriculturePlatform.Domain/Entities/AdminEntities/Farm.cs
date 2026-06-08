using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Common;

namespace AgriculturePlatform.Domain.Entities.AdminEntities;

public class Farm : BaseEntity
{
    public string FarmName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public decimal? TotalLandHectares { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LogoUrl { get; set; }
  
    
    // Navigation properties
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
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