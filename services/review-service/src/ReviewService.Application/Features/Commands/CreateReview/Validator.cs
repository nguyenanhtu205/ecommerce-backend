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

        RuleFor(x => x.MediaAttachments)
            .NotNull().WithMessage("Media attachments are required.")
            .Must(x => x.Count <= 4).WithMessage("Media attachments cannot exceed 4 items.");

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
    }
}
