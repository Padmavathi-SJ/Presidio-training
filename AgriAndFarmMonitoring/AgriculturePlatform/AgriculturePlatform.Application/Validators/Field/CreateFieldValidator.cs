// AgriculturePlatform.Application/Validators/CreateFieldValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class CreateFieldValidator : AbstractValidator<CreateFieldDto>
{
    public CreateFieldValidator()
    {
        // Field Name Validation
        RuleFor(x => x.FieldName)
            .NotEmpty().WithMessage("Field name is required")
            .MaximumLength(100).WithMessage("Field name must not exceed 100 characters")
            .MinimumLength(2).WithMessage("Field name must be at least 2 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Field name can only contain letters, numbers, spaces, hyphens and underscores");

        // Location Validation (Optional)
        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        // Area Hectares Validation
        RuleFor(x => x.AreaHectares)
            .GreaterThan(0).WithMessage("Area must be greater than 0 hectares")
            .LessThan(10000).WithMessage("Area cannot exceed 10,000 hectares")
            .PrecisionScale(10, 2, true).WithMessage("Area can have up to 2 decimal places")
            .When(x => x.AreaHectares.HasValue);

        // Soil Type Validation
        RuleFor(x => x.SoilType)
            .Must(BeValidSoilType).WithMessage($"Invalid soil type. Valid values: {string.Join(", ", Enum.GetNames<SoilTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.SoilType));

        // Status Validation
        RuleFor(x => x.Status)
            .Must(BeValidFieldStatus).WithMessage($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<FieldStatusEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // =============================================
        // LATITUDE & LONGITUDE VALIDATIONS - ADD THIS SECTION
        // =============================================
        
        // Latitude Validation
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees")
            .When(x => x.Latitude.HasValue);

        // Longitude Validation
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees")
            .When(x => x.Longitude.HasValue);

        // If one is provided, both should be provided
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Both Latitude and Longitude must be provided together or both empty");
    }

    private bool BeValidSoilType(string? soilType)
    {
        if (string.IsNullOrWhiteSpace(soilType)) return true;
        return Enum.TryParse<SoilTypeEnum>(soilType, true, out _);
    }

    private bool BeValidFieldStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return true;
        return Enum.TryParse<FieldStatusEnum>(status, true, out _);
    }
}