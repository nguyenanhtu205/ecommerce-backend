namespace ReviewService.Application.Features.Commands.LikeReview;

public record LikeReviewCommand(string ReviewId) : IRequest;

public class LikeReview(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<LikeReviewCommand>
{
    public async Task Handle(LikeReviewCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string buyerId = currentUser.UserId.Value.ToString();

        ReviewLike? existing = await context.ReviewLikes
            .Find(l => l.ReviewId == request.ReviewId && l.BuyerId == buyerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return;
        }

        ReviewLike like = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            ReviewId = request.ReviewId,
            BuyerId = buyerId,
            LikedAt = DateTimeOffset.UtcNow
        };

        await context.ReviewLikes.InsertOneAsync(like, cancellationToken: cancellationToken);

        await context.Reviews.UpdateOneAsync(
            r => r.Id == request.ReviewId,
            Builders<Review>.Update.Inc(r => r.LikeCount, 1),
            cancellationToken: cancellationToken);
    }
}
