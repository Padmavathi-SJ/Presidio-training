// AgriculturePlatform.Application/Validators/WeatherApiSettingsValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Validators;

public class WeatherApiSettingsValidator : AbstractValidator<WeatherApiSettingsDto>
{
    public WeatherApiSettingsValidator()
    {
        RuleFor(x => x.ApiProvider)
            .NotEmpty().WithMessage("API provider is required")
            .Must(x => x == "OpenWeatherMap" || x == "WeatherAPI" || x == "TomorrowIO")
            .WithMessage("API provider must be OpenWeatherMap, WeatherAPI, or TomorrowIO");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("API key is required")
            .MinimumLength(10).WithMessage("Invalid API key format");

        RuleFor(x => x.UpdateIntervalMinutes)
            .InclusiveBetween(15, 360).WithMessage("Update interval must be between 15 and 360 minutes");

        RuleFor(x => x.BaseUrl)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Invalid URL format")
            .When(x => !string.IsNullOrWhiteSpace(x.BaseUrl));
    }
}