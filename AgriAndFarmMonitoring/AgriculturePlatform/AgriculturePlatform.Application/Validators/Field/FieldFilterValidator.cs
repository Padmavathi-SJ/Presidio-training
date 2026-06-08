// AgriculturePlatform.Application/Validators/FieldFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class FieldFilterValidator : AbstractValidator<FieldFilterDto>
{
    public FieldFilterValidator()
    {
        // Soil Type Validation
        RuleFor(x => x.SoilType)
            .Must(BeValidSoilType).WithMessage($"Invalid soil type. Valid values: {string.Join(", ", Enum.GetNames<SoilTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.SoilType));

        // Status Validation
        RuleFor(x => x.Status)
            .Must(BeValidFieldStatus).WithMessage($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<FieldStatusEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // Pagination Validation
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1")
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100")
            .When(x => x.PageSize.HasValue);

        // SortBy Validation (optional - prevent SQL injection)
        RuleFor(x => x.SortBy)
            .Must(BeValidSortColumn).WithMessage("Invalid sort column")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        // Field Name search validation
        RuleFor(x => x.FieldName)
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FieldName));
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

    private bool BeValidSortColumn(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return true;
        
        // Allowed sort columns (prevent SQL injection)
        var allowedColumns = new[] 
        { 
            "Id", "FieldName", "Location", "AreaHectares", 
            "SoilType", "Status", "CreatedAt", "UpdatedAt" 
        };
        
        return allowedColumns.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}