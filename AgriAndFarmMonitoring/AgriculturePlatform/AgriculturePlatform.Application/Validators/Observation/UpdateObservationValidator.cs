// AgriculturePlatform.Application/Validators/UpdateObservationValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class UpdateObservationValidator : AbstractValidator<UpdateObservationDto>
{
    public UpdateObservationValidator()
    {
        RuleFor(x => x.ObservationDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Observation date cannot be in the future")
            .When(x => x.ObservationDate.HasValue);

        RuleFor(x => x.CropHealth)
            .Must(BeValidCropHealth).WithMessage($"Invalid crop health. Valid values: {string.Join(", ", Enum.GetNames<CropHealthEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.CropHealth));

        RuleFor(x => x.PestType)
            .NotEmpty().WithMessage("Pest type is required when pest is detected")
            .MaximumLength(100).WithMessage("Pest type cannot exceed 100 characters")
            .When(x => x.PestDetected.HasValue && x.PestDetected.Value);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
    }

    private bool BeValidCropHealth(string? cropHealth)
    {
        if (string.IsNullOrWhiteSpace(cropHealth)) return true;
        return Enum.TryParse<CropHealthEnum>(cropHealth, true, out _);
    }

    private bool AtLeastOneFieldProvided(UpdateObservationDto dto)
    {
        return dto.ObservationDate.HasValue ||
               !string.IsNullOrWhiteSpace(dto.CropHealth) ||
               dto.PestDetected.HasValue ||
               !string.IsNullOrWhiteSpace(dto.PestType) ||
               !string.IsNullOrWhiteSpace(dto.Notes) ||
               dto.ImageUrls != null;
    }
}