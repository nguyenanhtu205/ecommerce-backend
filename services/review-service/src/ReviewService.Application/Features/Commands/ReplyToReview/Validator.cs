namespace ReviewService.Application.Features.Commands.ReplyToReview;

public class Validator : AbstractValidator<ReplyToReviewCommand>
{
    public Validator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review id is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");
    }
}
