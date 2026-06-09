// AgriculturePlatform.Application/Validators/ObservationFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class ObservationFilterValidator : AbstractValidator<ObservationFilterDto>
{
    public ObservationFilterValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID must be greater than 0")
            .When(x => x.FieldId.HasValue);

        RuleFor(x => x.CropCycleId)
            .GreaterThan(0).WithMessage("Crop cycle ID must be greater than 0")
            .When(x => x.CropCycleId.HasValue);

        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("Worker ID must be greater than 0")
            .When(x => x.WorkerId.HasValue);

        RuleFor(x => x.CropHealth)
            .Must(BeValidCropHealth).WithMessage($"Invalid crop health. Valid values: {string.Join(", ", Enum.GetNames<CropHealthEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.CropHealth));

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate).WithMessage("From date must be less than or equal to To date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(200).WithMessage("Page size cannot exceed 200");
    }

    private bool BeValidCropHealth(string? cropHealth)
    {
        if (string.IsNullOrWhiteSpace(cropHealth)) return true;
        return Enum.TryParse<CropHealthEnum>(cropHealth, true, out _);
    }
}