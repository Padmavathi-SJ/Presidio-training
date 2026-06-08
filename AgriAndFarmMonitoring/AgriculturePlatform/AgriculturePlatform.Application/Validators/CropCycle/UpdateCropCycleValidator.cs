// AgriculturePlatform.Application/Validators/UpdateCropCycleValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class UpdateCropCycleValidator : AbstractValidator<UpdateCropCycleDto>
{
    public UpdateCropCycleValidator()
    {
        RuleFor(x => x.CropType)
            .Must(BeValidCropType).WithMessage($"Invalid crop type. Valid values: {string.Join(", ", Enum.GetNames<CropTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.CropType));

        RuleFor(x => x.ExpectedHarvestDate)
            .GreaterThan(x => x.PlantingDate).WithMessage("Expected harvest date must be after planting date")
            .When(x => x.ExpectedHarvestDate.HasValue && x.PlantingDate.HasValue);

        RuleFor(x => x.GrowthStage)
            .Must(BeValidGrowthStage).WithMessage($"Invalid growth stage. Valid values: {string.Join(", ", Enum.GetNames<GrowthStageEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.GrowthStage));

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage($"Invalid status. Valid values: ACTIVE, HARVESTED, FAILED")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
    }

    private bool BeValidCropType(string? cropType)
    {
        if (string.IsNullOrWhiteSpace(cropType)) return true;
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

    private bool AtLeastOneFieldProvided(UpdateCropCycleDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.CropType) ||
               dto.PlantingDate.HasValue ||
               dto.ExpectedHarvestDate.HasValue ||
               !string.IsNullOrWhiteSpace(dto.GrowthStage) ||
               !string.IsNullOrWhiteSpace(dto.Status);
    }
}