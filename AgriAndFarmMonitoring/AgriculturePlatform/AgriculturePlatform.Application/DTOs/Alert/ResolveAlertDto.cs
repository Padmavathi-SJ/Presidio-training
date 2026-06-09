// AgriculturePlatform.Application/DTOs/Alert/ResolveAlertDto.cs
namespace AgriculturePlatform.Application.DTOs.Alert;

public class ResolveAlertDto
{
    public int AlertId { get; set; }
    public string? ResolutionNotes { get; set; }
}