// AgriculturePlatform.Application/Validators/WeatherAlertCreateValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class WeatherAlertCreateValidator : AbstractValidator<WeatherAlertCreateDto>
{
    public WeatherAlertCreateValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID is required");

        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("Alert type is required")
            .Must(BeValidAlertType).WithMessage($"Invalid alert type. Valid values: {string.Join(", ", Enum.GetNames<WeatherAlertTypeEnum>())}");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .Must(BeValidSeverity).WithMessage($"Invalid severity. Valid values: {string.Join(", ", Enum.GetNames<WeatherAlertSeverityEnum>())}");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-50, 60).WithMessage("Temperature must be between -50°C and 60°C")
            .When(x => x.Temperature.HasValue);

        RuleFor(x => x.WindSpeed)
            .InclusiveBetween(0, 200).WithMessage("Wind speed must be between 0 and 200 km/h")
            .When(x => x.WindSpeed.HasValue);

        RuleFor(x => x.RainfallMm)
            .GreaterThanOrEqualTo(0).WithMessage("Rainfall cannot be negative")
            .LessThan(500).WithMessage("Rainfall cannot exceed 500mm")
            .When(x => x.RainfallMm.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }

    private bool BeValidAlertType(string alertType)
    {
        return Enum.TryParse<WeatherAlertTypeEnum>(alertType, true, out _);
    }

    private bool BeValidSeverity(string severity)
    {
        return Enum.TryParse<WeatherAlertSeverityEnum>(severity, true, out _);
    }
}