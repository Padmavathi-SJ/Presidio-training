// Application/Validators/QualityCheck/QualityCheckApprovalValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.QualityCheck;

namespace AgriculturePlatform.Application.Validators.QualityCheck;

public class QualityCheckApprovalValidator : AbstractValidator<QualityCheckApprovalDto>
{
    public QualityCheckApprovalValidator()
    {
        RuleFor(x => x.ApprovalStatus)
            .NotEmpty().WithMessage("Approval status is required")
            .Must(x => x == "APPROVED" || x == "REJECTED" || x == "REQUEST_CHANGES")
            .WithMessage("Approval status must be APPROVED, REJECTED, or REQUEST_CHANGES");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when rejecting")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters")
            .When(x => x.ApprovalStatus == "REJECTED");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000).WithMessage("Admin notes cannot exceed 1000 characters");
    }
}