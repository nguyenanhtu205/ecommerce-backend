namespace ReviewService.Application.Features.Commands.LikeReview;

public record LikeReviewCommand(string ReviewId) : IRequest;

public class LikeReview(IApplicationDbContext context) : IRequestHandler<LikeReviewCommand>
{
    public async Task Handle(LikeReviewCommand request, CancellationToken cancellationToken)
    {
        UpdateResult result = await context.Reviews.UpdateOneAsync(
            r => r.Id == request.ReviewId,
            Builders<Review>.Update.Inc(r => r.LikeCount, 1),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new NotFoundException($"Review '{request.ReviewId}' does not exist.");
        }
    }
}
