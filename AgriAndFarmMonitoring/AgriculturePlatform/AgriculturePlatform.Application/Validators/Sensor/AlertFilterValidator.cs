// AgriculturePlatform.Application/Validators/AlertFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class AlertFilterValidator : AbstractValidator<AlertFilterDto>
{
    public AlertFilterValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID must be greater than 0")
            .When(x => x.FieldId.HasValue);

        RuleFor(x => x.CropCycleId)
            .GreaterThan(0).WithMessage("Crop cycle ID must be greater than 0")
            .When(x => x.CropCycleId.HasValue);

        RuleFor(x => x.AlertType)
            .Must(BeValidAlertType).WithMessage($"Invalid alert type. Valid values: {string.Join(", ", Enum.GetNames<AlertTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.AlertType));

        RuleFor(x => x.Severity)
            .Must(BeValidSeverity).WithMessage($"Invalid severity. Valid values: {string.Join(", ", Enum.GetNames<AlertSeverityEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.Severity));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate).WithMessage("From date must be less than or equal to To date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(200).WithMessage("Page size cannot exceed 200");
    }

    private bool BeValidAlertType(string? alertType)
    {
        if (string.IsNullOrWhiteSpace(alertType)) return true;
        return Enum.TryParse<AlertTypeEnum>(alertType, true, out _);
    }

    private bool BeValidSeverity(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity)) return true;
        return Enum.TryParse<AlertSeverityEnum>(severity, true, out _);
    }
}