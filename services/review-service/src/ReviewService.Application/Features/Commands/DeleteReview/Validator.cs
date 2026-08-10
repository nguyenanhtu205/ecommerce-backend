namespace ReviewService.Application.Features.Commands.DeleteReview;

public class Validator : AbstractValidator<DeleteReviewCommand>
{
    public Validator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review id is required.");
    }
}
