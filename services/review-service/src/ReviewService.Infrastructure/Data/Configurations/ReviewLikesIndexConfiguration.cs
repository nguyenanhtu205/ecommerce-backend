namespace ReviewService.Infrastructure.Data.Configurations;

public class ReviewLikesIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<ReviewLike>> indexes =
        [
            new(
                Builders<ReviewLike>.IndexKeys
                    .Ascending(x => x.ReviewId)
                    .Ascending(x => x.BuyerId),
                new CreateIndexOptions { Name = "ux_review_likes_review_id_buyer_id", Unique = true })
        ];

        await context.ReviewLikes.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
