// AgriculturePlatform.Application/Validators/ManualWeatherEntryValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class ManualWeatherEntryValidator : AbstractValidator<ManualWeatherEntryDto>
{
    public ManualWeatherEntryValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID is required");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(-50, 60).WithMessage("Temperature must be between -50°C and 60°C")
            .When(x => x.Temperature.HasValue);

        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0% and 100%")
            .When(x => x.Humidity.HasValue);

        RuleFor(x => x.RainfallMm)
            .GreaterThanOrEqualTo(0).WithMessage("Rainfall cannot be negative")
            .LessThan(500).WithMessage("Rainfall cannot exceed 500mm")
            .When(x => x.RainfallMm.HasValue);

        RuleFor(x => x.WindSpeed)
            .InclusiveBetween(0, 200).WithMessage("Wind speed must be between 0 and 200 km/h")
            .When(x => x.WindSpeed.HasValue);

        RuleFor(x => x.Condition)
            .Must(BeValidCondition).WithMessage($"Invalid weather condition. Valid values: {string.Join(", ", Enum.GetNames<WeatherConditionEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.Condition));

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage("Recorded date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Recorded date cannot be in the future");
    }

    private bool BeValidCondition(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition)) return true;
        return Enum.TryParse<WeatherConditionEnum>(condition, true, out _);
    }
}