// Application/Validators/QualityCheck/CreateQualityCheckValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators.QualityCheck;

public class CreateQualityCheckValidator : AbstractValidator<CreateQualityCheckDto>
{
    public CreateQualityCheckValidator()
    {
        RuleFor(x => x.HarvestId)
            .GreaterThan(0).WithMessage("Harvest ID is required");

        RuleFor(x => x.CheckDate)
            .NotEmpty().WithMessage("Check date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Check date cannot be in the future");

        RuleFor(x => x.MoisturePct)
            .InclusiveBetween(0, 100).WithMessage("Moisture percentage must be between 0 and 100")
            .When(x => x.MoisturePct.HasValue);

        RuleFor(x => x.DefectPct)
            .InclusiveBetween(0, 100).WithMessage("Defect percentage must be between 0 and 100")
            .When(x => x.DefectPct.HasValue);

        RuleFor(x => x.FinalGrade)
            .Must(BeValidGrade).WithMessage($"Invalid grade. Valid values: {string.Join(", ", Enum.GetNames<QualityGradeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.FinalGrade));
    }

    private bool BeValidGrade(string? grade)
    {
        if (string.IsNullOrWhiteSpace(grade)) return true;
        return Enum.TryParse<QualityGradeEnum>(grade, true, out _);
    }
}