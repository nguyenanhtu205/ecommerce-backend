namespace ReviewService.Application.Features.Commands.CreateReview;

public class Validator : AbstractValidator<CreateReviewCommand>
{
    public Validator()
    {
        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item id is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");

        RuleFor(x => x.Attributes)
            .NotNull().WithMessage("Attributes must not be null.");

        RuleForEach(x => x.Attributes)
            .ChildRules(attribute =>
            {
                attribute.RuleFor(x => x.Label)
                    .NotEmpty().WithMessage("Attribute label is required.")
                    .MaximumLength(100).WithMessage("Attribute label must not exceed 100 characters.");

                attribute.RuleFor(x => x.Value)
                    .NotEmpty().WithMessage("Attribute value is required.")
                    .MaximumLength(255).WithMessage("Attribute value must not exceed 255 characters.");
            });

        RuleFor(x => x.MediaAssetIds)
            .NotNull().WithMessage("Media asset ids must not be null.")
            .Must(x => x.Count <= 10).WithMessage("You can attach up to 10 media assets per review.");
    }
}
