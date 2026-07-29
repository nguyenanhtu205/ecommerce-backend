namespace ProductCatalogService.Application.Features.Commands.CreateCategory;

public class Validator : AbstractValidator<CreateCategoryCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(255).WithMessage("Category name must not exceed 255 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(255).WithMessage("Slug must not exceed 255 characters.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase, alphanumeric, and hyphen-separated (e.g. 'mens-shoes').");

        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent id must not be empty when provided.")
            .When(x => x.ParentId is not null);

        RuleFor(x => x.Level)
            .GreaterThan(0).WithMessage("Level must be greater than 0.");

        RuleFor(x => x.Path)
            .NotNull().WithMessage("Path is required.");

        RuleForEach(x => x.Path)
            .ChildRules(path =>
            {
                path.RuleFor(x => x.Id)
                    .NotEmpty().WithMessage("Path item id is required.");

                path.RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Path item name is required.");
            });

        RuleFor(x => x.Level)
            .Equal(1).WithMessage("Root category (no parent) must have level 1.")
            .When(x => x.ParentId is null);

        RuleFor(x => x.Path)
            .Empty().WithMessage("Root category (no parent) must have an empty path.")
            .When(x => x.ParentId is null);
    }
}
