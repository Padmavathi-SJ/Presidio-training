// AgriculturePlatform.Application/Validators/Weather/WeatherAlertValidator.cs
using FluentValidation;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Validators.Weather;

public class WeatherAlertValidator : AbstractValidator<WeatherAlert>
{
    public WeatherAlertValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Alert title is required")
            .MaximumLength(200).WithMessage("Alert title cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("Alert message cannot exceed 1000 characters");

        RuleFor(x => x.AlertType)
            .IsInEnum().WithMessage("Invalid alert type");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Invalid severity level");
    }
}