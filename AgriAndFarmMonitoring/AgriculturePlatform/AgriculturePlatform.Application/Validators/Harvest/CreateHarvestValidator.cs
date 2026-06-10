// Application/Validators/Harvest/CreateHarvestValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators.Harvest;

public class CreateHarvestValidator : AbstractValidator<CreateHarvestDto>
{
    public CreateHarvestValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID is required");

        RuleFor(x => x.CropCycleId)
            .GreaterThan(0).WithMessage("Crop cycle ID is required");

        RuleFor(x => x.HarvestDate)
            .NotEmpty().WithMessage("Harvest date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Harvest date cannot be in the future");

        RuleFor(x => x.QuantityKg)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThan(1000000).WithMessage("Quantity cannot exceed 1,000,000 kg");

        RuleFor(x => x.QualityGrade)
            .Must(BeValidQualityGrade).WithMessage($"Invalid quality grade. Valid values: {string.Join(", ", Enum.GetNames<QualityGradeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.QualityGrade));

        RuleFor(x => x.HarvestMethod)
            .Must(BeValidHarvestMethod).WithMessage($"Invalid harvest method. Valid values: {string.Join(", ", Enum.GetNames<HarvestMethodEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.HarvestMethod));

        RuleFor(x => x.PricePerKg)
            .GreaterThanOrEqualTo(0).WithMessage("Price per kg cannot be negative")
            .LessThan(10000).WithMessage("Price per kg cannot exceed 10,000")
            .When(x => x.PricePerKg.HasValue);

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50).WithMessage("Batch number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }

    private bool BeValidQualityGrade(string? grade)
    {
        if (string.IsNullOrWhiteSpace(grade)) return true;
        return Enum.TryParse<QualityGradeEnum>(grade, true, out _);
    }

    private bool BeValidHarvestMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method)) return true;
        return Enum.TryParse<HarvestMethodEnum>(method, true, out _);
    }
}