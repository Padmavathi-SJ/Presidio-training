// Application/Validators/QualityCheck/UpdateQualityCheckValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators.QualityCheck;

public class UpdateQualityCheckValidator : AbstractValidator<UpdateQualityCheckDto>
{
    public UpdateQualityCheckValidator()
    {
        RuleFor(x => x.CheckDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Check date cannot be in the future")
            .When(x => x.CheckDate.HasValue);

        RuleFor(x => x.MoisturePct)
            .InclusiveBetween(0, 100).WithMessage("Moisture percentage must be between 0 and 100")
            .When(x => x.MoisturePct.HasValue);

        RuleFor(x => x.DefectPct)
            .InclusiveBetween(0, 100).WithMessage("Defect percentage must be between 0 and 100")
            .When(x => x.DefectPct.HasValue);

        RuleFor(x => x.FinalGrade)
            .Must(BeValidGrade).WithMessage($"Invalid grade. Valid values: {string.Join(", ", Enum.GetNames<QualityGradeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.FinalGrade));

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
    }

    private bool BeValidGrade(string? grade)
    {
        if (string.IsNullOrWhiteSpace(grade)) return true;
        return Enum.TryParse<QualityGradeEnum>(grade, true, out _);
    }

    private bool AtLeastOneFieldProvided(UpdateQualityCheckDto dto)
    {
        return dto.CheckDate.HasValue ||
               dto.MoisturePct.HasValue ||
               dto.DefectPct.HasValue ||
               !string.IsNullOrWhiteSpace(dto.FinalGrade) ||
               !string.IsNullOrWhiteSpace(dto.Notes);
    }
}