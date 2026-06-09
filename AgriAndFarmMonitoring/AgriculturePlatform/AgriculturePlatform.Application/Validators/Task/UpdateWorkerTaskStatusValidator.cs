// AgriculturePlatform.Application/Validators/UpdateWorkerTaskStatusValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.WorkerTask;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class UpdateWorkerTaskStatusValidator : AbstractValidator<UpdateWorkerTaskStatusDto>
{
    public UpdateWorkerTaskStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage($"Invalid status. Valid values: PENDING, IN_PROGRESS, COMPLETED");

        RuleFor(x => x.CompletionNotes)
            .MaximumLength(500).WithMessage("Completion notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CompletionNotes));
    }

    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "PENDING", "IN_PROGRESS", "COMPLETED" };
        return validStatuses.Contains(status.ToUpper());
    }
}