// AgriculturePlatform.Application/Validators/UpdateTaskValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.WorkerId)
            .GreaterThan(0).WithMessage("Worker ID must be greater than 0")
            .When(x => x.WorkerId.HasValue);

        RuleFor(x => x.TaskName)
            .Must(BeValidTaskType).WithMessage($"Invalid task type. Valid values: {string.Join(", ", Enum.GetNames<TaskTypeEnum>())}")
            .When(x => !string.IsNullOrWhiteSpace(x.TaskName));

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage($"Invalid status. Valid values: PENDING, IN_PROGRESS, COMPLETED, CANCELLED")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage($"Invalid priority. Valid values: LOW, MEDIUM, HIGH, URGENT")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
    }

    private bool BeValidTaskType(string? taskName)
    {
        if (string.IsNullOrWhiteSpace(taskName)) return true;
        return Enum.TryParse<TaskTypeEnum>(taskName, true, out _);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return true;
        return Enum.TryParse<TaskStatusEnum>(status, true, out _);
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority)) return true;
        return Enum.TryParse<TaskPriorityEnum>(priority, true, out _);
    }

    private bool AtLeastOneFieldProvided(UpdateTaskDto dto)
    {
        return dto.WorkerId.HasValue ||
               dto.FieldId.HasValue ||
               dto.CropCycleId.HasValue ||
               !string.IsNullOrWhiteSpace(dto.TaskName) ||
               dto.DueDate.HasValue ||
               !string.IsNullOrWhiteSpace(dto.Status) ||
               !string.IsNullOrWhiteSpace(dto.Priority) ||
               !string.IsNullOrWhiteSpace(dto.Notes);
    }
}