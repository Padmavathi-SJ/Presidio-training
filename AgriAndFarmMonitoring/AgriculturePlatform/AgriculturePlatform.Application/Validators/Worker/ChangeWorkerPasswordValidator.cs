// AgriculturePlatform.Application/Validators/ChangeWorkerPasswordValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Validators;

public class ChangeWorkerPasswordValidator : AbstractValidator<ChangeWorkerPasswordDto>
{
    public ChangeWorkerPasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters")
            .MaximumLength(50).WithMessage("New password must not exceed 50 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one number");
    }
}