// AgriculturePlatform.Application/Validators/BulkImportValidator.cs
using FluentValidation;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Validators;

public class BulkImportValidator : AbstractValidator<List<CreateFieldDto>>
{
    public BulkImportValidator()
    {
        RuleForEach(x => x)
            .SetValidator(new CreateFieldValidator());
        
        // Check for duplicate field names within the import
        RuleFor(x => x)
            .Must(HaveNoDuplicateNames)
            .WithMessage("Duplicate field names found in the import file");
        
        // Limit batch size
        RuleFor(x => x)
            .Must(x => x.Count <= 1000)
            .WithMessage("Cannot import more than 1000 fields at once");
    }

    private bool HaveNoDuplicateNames(List<CreateFieldDto> fields)
    {
        var names = fields.Select(f => f.FieldName.ToLowerInvariant());
        return names.Distinct().Count() == names.Count();
    }
}