// AgriculturePlatform.Application/Validators/AlertThresholdValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class CreateAlertThresholdValidator : AbstractValidator<CreateAlertThresholdDto>
{
    public CreateAlertThresholdValidator()
    {
        RuleFor(x => x.CropType)
            .NotEmpty().WithMessage("Crop type is required")
            .Must(BeValidCropType).WithMessage($"Invalid crop type. Valid values: {string.Join(", ", Enum.GetNames<CropTypeEnum>())}");

        RuleFor(x => x.GrowthStage)
            .NotEmpty().WithMessage("Growth stage is required")
            .Must(BeValidGrowthStage).WithMessage($"Invalid growth stage. Valid values: {string.Join(", ", Enum.GetNames<GrowthStageEnum>())}");

        RuleFor(x => x.SensorType)
            .NotEmpty().WithMessage("Sensor type is required")
            .Must(BeValidSensorType).WithMessage($"Invalid sensor type. Valid values: {string.Join(", ", Enum.GetNames<SensorTypeEnum>())}");

        RuleFor(x => x.MinValue)
            .LessThan(x => x.MaxValue).WithMessage("Min value must be less than Max value");

        RuleFor(x => x.MaxValue)
            .GreaterThan(x => x.MinValue).WithMessage("Max value must be greater than Min value");

        RuleFor(x => x.Severity)
            .Must(BeValidSeverity).WithMessage($"Invalid severity. Valid values: LOW, MEDIUM, HIGH, CRITICAL");

        RuleFor(x => x.NotificationEmails)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrWhiteSpace(x.NotificationEmails));
    }

    private bool BeValidCropType(string cropType)
    {
        return Enum.TryParse<CropTypeEnum>(cropType, true, out _);
    }

    private bool BeValidGrowthStage(string growthStage)
    {
        return Enum.TryParse<GrowthStageEnum>(growthStage, true, out _);
    }

    private bool BeValidSensorType(string sensorType)
    {
        return Enum.TryParse<SensorTypeEnum>(sensorType, true, out _);
    }

    private bool BeValidSeverity(string severity)
    {
        return Enum.TryParse<AlertSeverityEnum>(severity, true, out _);
    }
}