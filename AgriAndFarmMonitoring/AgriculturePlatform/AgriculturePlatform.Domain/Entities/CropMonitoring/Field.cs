using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.YieldReports;


namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Field : BaseEntity
{
    public int FarmId { get; set; }
    public int AdminId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public SoilTypeEnum? SoilType { get; set; }
    public FieldStatusEnum? Status { get; set; } = FieldStatusEnum.ACTIVE;

     public double? Latitude { get; set; }  
    public double? Longitude { get; set; }  
    
   
    
    // Navigation properties
    public virtual Farm? Farm { get; set; }
    public virtual Admin? Admin { get; set; }
    public virtual ICollection<CropCycle> CropCycles { get; set; } = new List<CropCycle>();
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<WeatherData> WeatherData { get; set; } = new List<WeatherData>();
    public virtual ICollection<WorkerTask> Tasks { get; set; } = new List<WorkerTask>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
}