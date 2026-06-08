// AgriculturePlatform.Application/Validators/WorkerFieldFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.WorkerField;

namespace AgriculturePlatform.Application.Validators;

public class WorkerFieldFilterValidator : AbstractValidator<WorkerFieldFilterDto>
{
    public WorkerFieldFilterValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("Worker ID must be greater than 0")
            .When(x => x.WorkerId.HasValue);

        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID must be greater than 0")
            .When(x => x.FieldId.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1")
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.AssignedDateFrom)
            .LessThanOrEqualTo(x => x.AssignedDateTo).WithMessage("From date must be less than or equal to To date")
            .When(x => x.AssignedDateFrom.HasValue && x.AssignedDateTo.HasValue);
    }
}