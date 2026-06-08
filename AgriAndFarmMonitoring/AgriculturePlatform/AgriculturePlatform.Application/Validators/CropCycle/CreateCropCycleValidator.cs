// AgriculturePlatform.Application/Validators/CreateCropCycleValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class CreateCropCycleValidator : AbstractValidator<CreateCropCycleDto>
{
    public CreateCropCycleValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("FieldId is required");

        RuleFor(x => x.CropType)
            .NotEmpty().WithMessage("Crop type is required")
            .Must(BeValidCropType).WithMessage($"Invalid crop type. Valid values: {string.Join(", ", Enum.GetNames<CropTypeEnum>())}");

        RuleFor(x => x.PlantingDate)
            .NotEmpty().WithMessage("Planting date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Planting date cannot be in the future");

        RuleFor(x => x.ExpectedHarvestDate)
            .GreaterThan(x => x.PlantingDate).WithMessage("Expected harvest date must be after planting date")
            .When(x => x.ExpectedHarvestDate.HasValue);

        RuleFor(x => x.GrowthStage)
            .Must(BeValidGrowthStage).WithMessage($"Invalid growth stage. Valid values: {string.Join(", ", Enum.GetNames<GrowthStageEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.GrowthStage));

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage($"Invalid status. Valid values: ACTIVE, HARVESTED, FAILED")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }

    private bool BeValidCropType(string cropType)
    {
        return Enum.TryParse<CropTypeEnum>(cropType, true, out _);
    }

    private bool BeValidGrowthStage(string? growthStage)
    {
        if (string.IsNullOrWhiteSpace(growthStage)) return true;
        return Enum.TryParse<GrowthStageEnum>(growthStage, true, out _);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return true;
        return status.ToUpper() == "ACTIVE" || status.ToUpper() == "HARVESTED" || status.ToUpper() == "FAILED";
    }
}