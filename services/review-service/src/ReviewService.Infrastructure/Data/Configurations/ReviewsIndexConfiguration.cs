namespace ReviewService.Infrastructure.Data.Configurations;

public class ReviewsIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<Review>> indexes =
        [
            new(
                Builders<Review>.IndexKeys.Ascending(x => x.OrderItemId),
                new CreateIndexOptions { Unique = true, Name = "ux_reviews_order_item_id" }),

            new(
                Builders<Review>.IndexKeys
                    .Ascending(x => x.ProductId)
                    .Descending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "ix_reviews_product_id_created_at" }),

            new(
                Builders<Review>.IndexKeys
                    .Ascending(x => x.ShopId)
                    .Descending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "ix_reviews_shop_id_created_at" }),

            new(
                Builders<Review>.IndexKeys
                    .Ascending(x => x.BuyerId)
                    .Descending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "ix_reviews_buyer_id_created_at" })
        ];

        await context.Reviews.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
