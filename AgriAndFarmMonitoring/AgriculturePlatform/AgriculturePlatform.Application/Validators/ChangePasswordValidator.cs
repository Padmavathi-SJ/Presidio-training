// Application/Validators/ChangePasswordValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Admin;

namespace AgriculturePlatform.Application.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .MaximumLength(50).WithMessage("New password must not exceed 50 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one number")
            .Matches(@"[@$!%*?&]").WithMessage("New password must contain at least one special character (@$!%*?&)")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Please confirm your new password")
            .Equal(x => x.NewPassword).WithMessage("New password and confirmation password do not match");
    }
}