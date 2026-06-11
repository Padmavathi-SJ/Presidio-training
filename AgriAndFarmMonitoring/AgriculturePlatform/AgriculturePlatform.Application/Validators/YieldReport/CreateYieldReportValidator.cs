// Application/Validators/YieldReport/CreateYieldReportValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.YieldReport;

namespace AgriculturePlatform.Application.Validators.YieldReport;

public class CreateYieldReportValidator : AbstractValidator<CreateYieldReportDto>
{
    public CreateYieldReportValidator()
    {
        RuleFor(x => x.ReportName)
            .NotEmpty().WithMessage("Report name is required")
            .MaximumLength(200).WithMessage("Report name cannot exceed 200 characters");

        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Report type is required")
            .Must(x => new[] { "DAILY", "WEEKLY", "MONTHLY", "SEASONAL", "YEARLY", "CUSTOM" }.Contains(x))
            .WithMessage("Invalid report type");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.ScheduleCron)
            .NotEmpty().WithMessage("Cron expression is required for scheduled reports")
            .When(x => x.IsScheduled);
    }
}