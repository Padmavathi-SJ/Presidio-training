// AgriculturePlatform.Application/Validators/WorkerLoginDtoValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Validators;

public class WorkerLoginDtoValidator : AbstractValidator<WorkerLoginDto>
{
    public WorkerLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}