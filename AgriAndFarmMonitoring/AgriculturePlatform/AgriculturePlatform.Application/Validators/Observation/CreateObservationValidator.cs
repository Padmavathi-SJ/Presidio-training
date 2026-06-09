// AgriculturePlatform.Application/Validators/CreateObservationValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class CreateObservationValidator : AbstractValidator<CreateObservationDto>
{
    public CreateObservationValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID is required");

        RuleFor(x => x.ObservationDate)
            .NotEmpty().WithMessage("Observation date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Observation date cannot be in the future");

        RuleFor(x => x.CropHealth)
            .Must(BeValidCropHealth).WithMessage($"Invalid crop health. Valid values: {string.Join(", ", Enum.GetNames<CropHealthEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.CropHealth));

        RuleFor(x => x.PestType)
            .NotEmpty().WithMessage("Pest type is required when pest is detected")
            .MaximumLength(100).WithMessage("Pest type cannot exceed 100 characters")
            .When(x => x.PestDetected);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.ImageUrls)
            .Must(images => images == null || images.Count <= 10)
            .WithMessage("Maximum 10 images allowed per observation");
    }

    private bool BeValidCropHealth(string? cropHealth)
    {
        if (string.IsNullOrWhiteSpace(cropHealth)) return true;
        return Enum.TryParse<CropHealthEnum>(cropHealth, true, out _);
    }
}