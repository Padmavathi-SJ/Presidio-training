using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring;

public class Field
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public SoilTypeEnum? SoilType { get; set; }
    public FieldStatusEnum? Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<CropCycle> CropCycles { get; set; } = new List<CropCycle>();
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public virtual ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public virtual ICollection<WeatherData> WeatherData { get; set; } = new List<WeatherData>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
}