namespace AgriculturePlatform.Application.DTOs.Admin;

public class TokenValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public int? AdminId { get; set; }
    public int? FarmId { get; set; }
}