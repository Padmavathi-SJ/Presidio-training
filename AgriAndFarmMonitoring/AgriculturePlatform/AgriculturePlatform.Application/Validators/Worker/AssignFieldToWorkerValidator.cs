// AgriculturePlatform.Application/Validators/AssignFieldToWorkerValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.WorkerField;

namespace AgriculturePlatform.Application.Validators;

public class AssignFieldToWorkerValidator : AbstractValidator<AssignFieldToWorkerDto>
{
    public AssignFieldToWorkerValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("Worker ID is required and must be greater than 0");

        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID is required and must be greater than 0");

        RuleFor(x => x.AssignedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Assigned date cannot be in the future")
            .When(x => x.AssignedDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.AssignedDate).WithMessage("End date must be after the assigned date")
            .When(x => x.EndDate.HasValue && x.AssignedDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}