// AgriculturePlatform.Application/Validators/UpdateWorkerProfileValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Validators;

public class UpdateWorkerProfileValidator : AbstractValidator<UpdateWorkerProfileDto>
{
    public UpdateWorkerProfileValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^[0-9+\-\s]+$").WithMessage("Phone contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        // Password validation - if new password is provided, current password must also be provided
        When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required to set a new password");

            RuleFor(x => x.NewPassword)
                .MinimumLength(6).WithMessage("New password must be at least 6 characters")
                .MaximumLength(50).WithMessage("New password must not exceed 50 characters")
                .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("New password must contain at least one number");
        });
    }
}