// AgriculturePlatform.Application/DTOs/Field/FieldDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class FieldDto
{
    public int Id { get; set; }
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; }
    public int ActiveCropCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Soft delete fields
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Weather/Location fields - ADD THESE
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}