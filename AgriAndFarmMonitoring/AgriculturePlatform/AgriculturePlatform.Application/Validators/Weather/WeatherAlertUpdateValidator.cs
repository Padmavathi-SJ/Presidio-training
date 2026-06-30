// AgriculturePlatform.Application/Validators/WeatherAlertUpdateValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class WeatherAlertUpdateValidator : AbstractValidator<WeatherAlertUpdateDto>
{
    public WeatherAlertUpdateValidator()
    {
        RuleFor(x => x.Severity)
            .Must(BeValidSeverity).WithMessage($"Invalid severity. Valid values: {string.Join(", ", Enum.GetNames<WeatherAlertSeverityEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.Severity));

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Message));

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }

    private bool BeValidSeverity(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity)) return true;
        return Enum.TryParse<WeatherAlertSeverityEnum>(severity, true, out _);
    }
}