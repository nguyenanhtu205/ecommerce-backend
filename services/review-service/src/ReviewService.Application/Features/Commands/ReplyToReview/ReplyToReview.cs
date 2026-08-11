namespace ReviewService.Application.Features.Commands.ReplyToReview;

public record ReplyToReviewCommand(string ReviewId, string Content) : IRequest;

public class ReplyToReview(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<ReplyToReviewCommand>
{
    public async Task Handle(ReplyToReviewCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Review review = await context.Reviews
                            .Find(r => r.Id == request.ReviewId)
                            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException("Review not found");

        if (review.ShopId != currentUser.ShopId.Value.ToString())
        {
            throw new ForbiddenAccessException();
        }

        if (review.SellerReply is not null)
        {
            throw new ConflictException("This review has already been replied to.");
        }

        Reply sellerReply = new() { Content = request.Content, RepliedAt = DateTimeOffset.UtcNow };

        await context.Reviews.UpdateOneAsync(
            r => r.Id == request.ReviewId,
            Builders<Review>.Update.Set(r => r.SellerReply, sellerReply),
            cancellationToken: cancellationToken);
    }
}
