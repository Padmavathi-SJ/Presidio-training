// AgriculturePlatform.Application/DTOs/Weather/WeatherHistoryFilterDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherHistoryFilterDto
{
    public int? FieldId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 30;
    public List<int>? AllowedFieldIds { get; set; }
}