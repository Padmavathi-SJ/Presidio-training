using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Domain.Entities.YieldReports;

public class Harvest
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int CropCycleId { get; set; }
    public int? HarvestedBy { get; set; }
    public DateTime HarvestDate { get; set; }
    public decimal QuantityKg { get; set; }
    public QualityGradeEnum? QualityGrade { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Field? Field { get; set; }
    public virtual CropCycle? CropCycle { get; set; }
    public virtual Worker? Harvester { get; set; }
    public virtual ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
}