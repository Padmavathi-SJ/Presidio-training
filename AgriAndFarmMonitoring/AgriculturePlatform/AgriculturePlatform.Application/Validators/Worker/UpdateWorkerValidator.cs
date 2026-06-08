// AgriculturePlatform.Application/Validators/UpdateWorkerValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Validators;

public class UpdateWorkerValidator : AbstractValidator<UpdateWorkerDto>
{
    public UpdateWorkerValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^[0-9+\-\s]+$").WithMessage("Phone contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Role)
            .Must(BeValidRole).WithMessage("Invalid role. Valid values: MANAGER, SUPERVISOR, OPERATOR, LABOR, TECHNICIAN, DRIVER")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x)
            .Must(AtLeastOneFieldProvided)
            .WithMessage("At least one field must be provided for update");
    }

    private bool BeValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return true;
        var validRoles = new[] { "MANAGER", "SUPERVISOR", "OPERATOR", "LABOR", "TECHNICIAN", "DRIVER" };
        return validRoles.Contains(role.ToUpper());
    }

    private bool AtLeastOneFieldProvided(UpdateWorkerDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name) ||
               !string.IsNullOrWhiteSpace(dto.Email) ||
               !string.IsNullOrWhiteSpace(dto.Phone) ||
               !string.IsNullOrWhiteSpace(dto.Role) ||
               dto.IsActive.HasValue;
    }
}