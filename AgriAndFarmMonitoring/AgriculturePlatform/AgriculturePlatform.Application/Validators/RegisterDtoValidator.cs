// AgriculturePlatform.Application/Validators/RegisterDtoValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Admin;

namespace AgriculturePlatform.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        // Farm Validations
        RuleFor(x => x.FarmName)
            .NotEmpty().WithMessage("Farm name is required")
            .MaximumLength(200).WithMessage("Farm name must not exceed 200 characters");

        RuleFor(x => x.FarmEmail)
            .NotEmpty().WithMessage("Farm email is required")
            .EmailAddress().WithMessage("Invalid farm email format")
            .MaximumLength(100).WithMessage("Farm email must not exceed 100 characters");

        RuleFor(x => x.FarmPhone)
            .MaximumLength(20).WithMessage("Farm phone must not exceed 20 characters");

        RuleFor(x => x.TotalLandHectares)
            .GreaterThan(0).WithMessage("Total land hectares must be greater than 0")
            .When(x => x.TotalLandHectares.HasValue);

        // Admin Validations
        RuleFor(x => x.AdminName)
            .NotEmpty().WithMessage("Admin name is required")
            .MaximumLength(100).WithMessage("Admin name must not exceed 100 characters");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required")
            .EmailAddress().WithMessage("Invalid admin email format")
            .MaximumLength(100).WithMessage("Admin email must not exceed 100 characters");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(50).WithMessage("Password must not exceed 50 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.AdminPhone)
            .MaximumLength(20).WithMessage("Admin phone must not exceed 20 characters");
    }
}