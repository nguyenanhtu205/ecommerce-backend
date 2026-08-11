namespace ReviewService.Application.Features.Commands.UnlikeReview;

public class Validator : AbstractValidator<UnlikeReviewCommand>
{
    public Validator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review id is required.");
    }
}
