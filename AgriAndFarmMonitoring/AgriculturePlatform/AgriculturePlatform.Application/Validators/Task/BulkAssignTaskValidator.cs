// AgriculturePlatform.Application/Validators/BulkAssignTaskValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class BulkAssignTaskValidator : AbstractValidator<BulkAssignTaskDto>
{
    public BulkAssignTaskValidator()
    {
        RuleFor(x => x.WorkerIds)
            .NotEmpty().WithMessage("At least one worker ID is required")
            .Must(x => x.Count <= 50).WithMessage("Cannot assign to more than 50 workers at once");

        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("Task name is required")
            .Must(BeValidTaskType).WithMessage($"Invalid task type. Valid values: {string.Join(", ", Enum.GetNames<TaskTypeEnum>())}");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage($"Invalid priority. Valid values: LOW, MEDIUM, HIGH, URGENT")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));
    }

    private bool BeValidTaskType(string taskName)
    {
        return Enum.TryParse<TaskTypeEnum>(taskName, true, out _);
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority)) return true;
        return Enum.TryParse<TaskPriorityEnum>(priority, true, out _);
    }
}