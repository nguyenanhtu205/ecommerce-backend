namespace ReviewService.Application.Features.Commands.LikeReview;

public class Validator : AbstractValidator<LikeReviewCommand>
{
    public Validator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review id is required.");
    }
}
