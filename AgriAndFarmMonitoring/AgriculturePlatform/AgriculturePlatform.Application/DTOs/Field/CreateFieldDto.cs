// AgriculturePlatform.Application/DTOs/Field/CreateFieldDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class CreateFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; } = "ACTIVE";
    
    // Weather/Location fields - ADD THESE
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Image attachments
    public string? ImagePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ImageCaption { get; set; }
    public System.Collections.Generic.List<string>? AdditionalImagePaths { get; set; }
}