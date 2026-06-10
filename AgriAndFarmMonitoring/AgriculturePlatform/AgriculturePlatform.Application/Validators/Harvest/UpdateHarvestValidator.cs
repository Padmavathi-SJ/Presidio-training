// Application/Validators/Harvest/UpdateHarvestValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators.Harvest;

public class UpdateHarvestValidator : AbstractValidator<UpdateHarvestDto>
{
    public UpdateHarvestValidator()
    {
        RuleFor(x => x.HarvestDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Harvest date cannot be in the future")
            .When(x => x.HarvestDate.HasValue);

        RuleFor(x => x.QuantityKg)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThan(1000000).WithMessage("Quantity cannot exceed 1,000,000 kg")
            .When(x => x.QuantityKg.HasValue);

        RuleFor(x => x.QualityGrade)
            .Must(BeValidQualityGrade).WithMessage($"Invalid quality grade. Valid values: {string.Join(", ", Enum.GetNames<QualityGradeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.QualityGrade));

        RuleFor(x => x.HarvestMethod)
            .Must(BeValidHarvestMethod).WithMessage($"Invalid harvest method. Valid values: {string.Join(", ", Enum.GetNames<HarvestMethodEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.HarvestMethod));

        RuleFor(x => x.PricePerKg)
            .GreaterThanOrEqualTo(0).WithMessage("Price per kg cannot be negative")
            .When(x => x.PricePerKg.HasValue);

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
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

    private bool AtLeastOneFieldProvided(UpdateHarvestDto dto)
    {
        return dto.HarvestDate.HasValue ||
               dto.QuantityKg.HasValue ||
               !string.IsNullOrWhiteSpace(dto.QualityGrade) ||
               !string.IsNullOrWhiteSpace(dto.HarvestMethod) ||
               !string.IsNullOrWhiteSpace(dto.Notes) ||
               dto.PricePerKg.HasValue ||
               !string.IsNullOrWhiteSpace(dto.BatchNumber);
    }
}