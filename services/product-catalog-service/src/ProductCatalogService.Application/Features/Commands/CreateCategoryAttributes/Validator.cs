namespace ProductCatalogService.Application.Features.Commands.CreateCategoryAttributes;

public class Validator : AbstractValidator<CreateCategoryAttributesCommand>
{
    private static readonly string[] AllowedInputTypes =
    [
        "text", "number", "boolean", "select", "multiselect", "date"
    ];

    private static readonly string[] OptionsRequiredInputTypes =
    [
        "select", "multiselect"
    ];

    public Validator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(255).WithMessage("Attribute name must not exceed 255 characters.");

        RuleFor(x => x.InputType)
            .NotEmpty().WithMessage("Input type is required.")
            .Must(t => AllowedInputTypes.Contains(t))
            .WithMessage($"Input type must be one of: {string.Join(", ", AllowedInputTypes)}.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Options are required when input type is 'select' or 'multiselect'.")
            .When(x => OptionsRequiredInputTypes.Contains(x.InputType));

        RuleFor(x => x.Options)
            .Must(o => o.Distinct(StringComparer.OrdinalIgnoreCase).Count() == o.Count)
            .WithMessage("Options must not contain duplicate values.")
            .When(x => x.Options is { Count: > 0 });

        RuleFor(x => x.CompletionWeight)
            .InclusiveBetween(0, 100).WithMessage("Completion weight must be between 0 and 100.");

        RuleFor(x => x.SortOrder)
            .GreaterThan(0).WithMessage("Sort order must be greater than 0.");
    }
}
