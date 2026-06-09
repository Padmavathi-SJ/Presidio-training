// AgriculturePlatform.Application/Validators/SensorReadingFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class SensorReadingFilterValidator : AbstractValidator<SensorReadingFilterDto>
{
    public SensorReadingFilterValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID must be greater than 0")
            .When(x => x.FieldId.HasValue);

        RuleFor(x => x.CropCycleId)
            .GreaterThan(0).WithMessage("Crop cycle ID must be greater than 0")
            .When(x => x.CropCycleId.HasValue);

        RuleFor(x => x.SensorType)
            .Must(BeValidSensorType).WithMessage($"Invalid sensor type. Valid values: {string.Join(", ", Enum.GetNames<SensorTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.SensorType));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate).WithMessage("From date must be less than or equal to To date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(500).WithMessage("Page size cannot exceed 500");

        RuleFor(x => x.GroupBy)
            .Must(g => string.IsNullOrWhiteSpace(g) || new[] { "day", "week", "month" }.Contains(g.ToLower()))
            .WithMessage("Group by must be 'day', 'week', or 'month'");
    }

    private bool BeValidSensorType(string? sensorType)
    {
        if (string.IsNullOrWhiteSpace(sensorType)) return true;
        return Enum.TryParse<SensorTypeEnum>(sensorType, true, out _);
    }
}