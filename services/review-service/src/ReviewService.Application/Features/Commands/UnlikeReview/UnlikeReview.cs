namespace ReviewService.Application.Features.Commands.UnlikeReview;

public record UnlikeReviewCommand(string ReviewId) : IRequest;

public class UnlikeReview(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<UnlikeReviewCommand>
{
    public async Task Handle(UnlikeReviewCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string buyerId = currentUser.UserId.Value.ToString();

        DeleteResult result = await context.ReviewLikes.DeleteOneAsync(
            l => l.ReviewId == request.ReviewId && l.BuyerId == buyerId,
            cancellationToken);

        if (result.DeletedCount == 0)
        {
            return;
        }

        await context.Reviews.UpdateOneAsync(
            r => r.Id == request.ReviewId,
            Builders<Review>.Update.Inc(r => r.LikeCount, -1),
            cancellationToken: cancellationToken);
    }
}
