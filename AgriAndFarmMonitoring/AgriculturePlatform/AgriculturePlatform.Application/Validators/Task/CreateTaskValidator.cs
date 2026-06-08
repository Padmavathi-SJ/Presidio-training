// AgriculturePlatform.Application/Validators/CreateTaskValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("Worker ID is required");

        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("Task name is required")
            .Must(BeValidTaskType).WithMessage($"Invalid task type. Valid values: {string.Join(", ", Enum.GetNames<TaskTypeEnum>())}");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage($"Invalid priority. Valid values: LOW, MEDIUM, HIGH, URGENT")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
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