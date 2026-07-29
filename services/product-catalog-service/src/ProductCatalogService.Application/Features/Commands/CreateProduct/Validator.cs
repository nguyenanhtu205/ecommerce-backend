namespace ProductCatalogService.Application.Features.Commands.CreateProduct;

public class Validator : AbstractValidator<CreateProductCommand>
{
    public Validator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("ShopId is required.");

        RuleFor(x => x.ShopName)
            .NotEmpty().WithMessage("ShopName is required.")
            .MaximumLength(200).WithMessage("ShopName must not exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20).WithMessage("Tags must not exceed 20 items.");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag must not be empty.")
            .MaximumLength(50).WithMessage("Tag must not exceed 50 characters.");

        RuleFor(x => x.Condition)
            .IsInEnum().WithMessage("Condition is invalid.");

        RuleFor(x => x.MediaAttachments)
            .NotEmpty().WithMessage("At least one media attachment is required.");

        RuleForEach(x => x.MediaAttachments)
            .ChildRules(media =>
            {
                media.RuleFor(m => m.MediaAssetId)
                    .NotEmpty().WithMessage("MediaAssetId is required.");

                media.RuleFor(m => m.Role)
                    .NotEmpty().WithMessage("Role is required.");

                media.RuleFor(m => m.Position)
                    .GreaterThanOrEqualTo(0).WithMessage("Position must be greater than or equal to 0.");
            });

        RuleFor(x => x.ThumbnailMediaId)
            .NotEmpty().WithMessage("ThumbnailMediaId is required.");

        RuleFor(x => x.GalleryMediaIds)
            .NotNull().WithMessage("GalleryMediaIds must not be null.");

        RuleFor(x => x.Specifications)
            .NotNull().WithMessage("Specifications must not be null.");

        RuleForEach(x => x.Specifications).ChildRules(spec =>
        {
            spec.RuleFor(s => s.AttributeId)
                .NotEmpty().WithMessage("Specification AttributeId is required.");

            spec.RuleFor(s => s.Title)
                .NotEmpty().WithMessage("Specification Title is required.");

            spec.RuleFor(s => s.Value)
                .NotEmpty().WithMessage("Specification Value is required.");
        });

        RuleFor(x => x.VariantGroups)
            .NotNull().WithMessage("VariantGroups must not be null.");

        RuleForEach(x => x.VariantGroups).ChildRules(group =>
        {
            group.RuleFor(g => g.Name)
                .NotEmpty().WithMessage("VariantGroup Name is required.");

            group.RuleFor(g => g.Options)
                .NotEmpty().WithMessage("VariantGroup must have at least one option.");

            group.RuleForEach(g => g.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Value)
                    .NotEmpty().WithMessage("VariantOption Value is required.");
            });
        });

        RuleFor(x => x.VariantCombinations)
            .NotEmpty().WithMessage("At least one variant combination is required.");

        RuleForEach(x => x.VariantCombinations).ChildRules(combination =>
        {
            combination.RuleFor(c => c.OptionValues)
                .NotEmpty().WithMessage("VariantCombination OptionValues is required.");

            combination.RuleFor(c => c.Sku)
                .NotEmpty().WithMessage("VariantCombination Sku is required.");

            combination.RuleFor(c => c.InitialPrice)
                .GreaterThan(0).WithMessage("VariantCombination InitialPrice must be greater than 0.");

            combination.RuleFor(c => c.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("VariantCombination InitialStock must not be negative.");
        });

        RuleFor(x => x.ShippingInfo)
            .NotNull().WithMessage("ShippingInfo is required.");

        RuleFor(x => x.ShippingInfo.WeightGrams)
            .GreaterThan(0).WithMessage("WeightGrams must be greater than 0.");

        RuleFor(x => x.ShippingInfo.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.");

        RuleFor(x => x.ShippingInfo.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(x => x.ShippingInfo.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");

        RuleFor(x => x.PreOrderDays)
            .GreaterThan(0).WithMessage("PreOrderDays must be greater than 0.")
            .When(x => x.IsPreOrder);

        RuleFor(x => x.PreOrderDays)
            .Null().WithMessage("PreOrderDays must be empty when IsPreOrder is false.")
            .When(x => !x.IsPreOrder);
    }
}
