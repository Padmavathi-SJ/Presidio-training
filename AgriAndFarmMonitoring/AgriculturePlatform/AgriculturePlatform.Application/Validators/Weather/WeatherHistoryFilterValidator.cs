// AgriculturePlatform.Application/Validators/WeatherHistoryFilterValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Validators;

public class WeatherHistoryFilterValidator : AbstractValidator<WeatherHistoryFilterDto>
{
    public WeatherHistoryFilterValidator()
    {
        RuleFor(x => x.FieldId)
            .GreaterThan(0).WithMessage("Field ID must be greater than 0")
            .When(x => x.FieldId.HasValue);

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("From date cannot be in the future")
            .When(x => x.FromDate.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("To date must be after from date")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0")
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100")
            .When(x => x.PageSize.HasValue);
    }
}