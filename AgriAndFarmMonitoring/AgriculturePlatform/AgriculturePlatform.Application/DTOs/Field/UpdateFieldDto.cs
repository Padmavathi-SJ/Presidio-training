// AgriculturePlatform.Application/DTOs/Field/UpdateFieldDto.cs
namespace AgriculturePlatform.Application.DTOs.Field;

public class UpdateFieldDto
{
    public string? FieldName { get; set; }
    public string? Location { get; set; }
    public decimal? AreaHectares { get; set; }
    public string? SoilType { get; set; }
    public string? Status { get; set; }
    
    // Weather/Location fields - ADD THESE
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Image attachments
    public string? ImagePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ImageCaption { get; set; }
    public System.Collections.Generic.List<string>? AdditionalImagePaths { get; set; }
}