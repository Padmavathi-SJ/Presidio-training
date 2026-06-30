// AgriculturePlatform.Application/DTOs/Weather/AcknowledgeWeatherAlertDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class AcknowledgeWeatherAlertDto
{
    public int AlertId { get; set; }
    public string? Notes { get; set; }
}