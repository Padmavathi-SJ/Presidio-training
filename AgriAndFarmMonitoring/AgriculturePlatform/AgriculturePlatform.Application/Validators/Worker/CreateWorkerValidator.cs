// AgriculturePlatform.Application/Validators/CreateWorkerValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Validators;

public class CreateWorkerValidator : AbstractValidator<CreateWorkerDto>
{
    public CreateWorkerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^[0-9+\-\s]+$").WithMessage("Phone contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Role)
            .Must(BeValidRole).WithMessage("Invalid role. Valid values: MANAGER, SUPERVISOR, OPERATOR, LABOR, TECHNICIAN, DRIVER")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(50).WithMessage("Password must not exceed 50 characters");

        RuleFor(x => x.HireDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Hire date cannot be in the future")
            .When(x => x.HireDate.HasValue);
    }

    private bool BeValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return true;
        var validRoles = new[] { "MANAGER", "SUPERVISOR", "OPERATOR", "LABOR", "TECHNICIAN", "DRIVER" };
        return validRoles.Contains(role.ToUpper());
    }
}