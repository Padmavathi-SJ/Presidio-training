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
            .MaximumLength(100).WithMessage("Pest type cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PestType));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.AdditionalImagePaths)
            .Must(images => images == null || images.Count <= 10)
            .WithMessage("Maximum 10 additional images allowed per observation");

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
               !string.IsNullOrWhiteSpace(dto.PestType) ||
               !string.IsNullOrWhiteSpace(dto.Notes) ||
               !string.IsNullOrWhiteSpace(dto.ImagePath) ||
               !string.IsNullOrWhiteSpace(dto.ThumbnailPath) ||
               dto.AdditionalImagePaths != null ||
               !string.IsNullOrWhiteSpace(dto.ImageMetadata);
    }
}