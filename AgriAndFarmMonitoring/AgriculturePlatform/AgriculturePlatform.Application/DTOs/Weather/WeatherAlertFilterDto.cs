// AgriculturePlatform.Application/DTOs/Weather/WeatherAlertFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherAlertFilterDto
{
    public int? FieldId { get; set; }
    public string? Severity { get; set; }
    public bool? IsAcknowledged { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = true;
}