namespace ReviewService.Infrastructure.Data.Configurations;

public class ReviewableOrderItemsIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<ReviewableOrderItem>> indexes =
        [
            new(
                Builders<ReviewableOrderItem>.IndexKeys
                    .Ascending(x => x.BuyerId)
                    .Ascending(x => x.IsReviewed)
                    .Descending(x => x.OrderCompletedAt),
                new CreateIndexOptions { Name = "ix_reviewable_order_items_buyer_id_is_reviewed_completed_at" }),

            new(
                Builders<ReviewableOrderItem>.IndexKeys.Ascending(x => x.ProductId),
                new CreateIndexOptions { Name = "ix_reviewable_order_items_product_id" }),

            new(
                Builders<ReviewableOrderItem>.IndexKeys.Ascending(x => x.ShopId),
                new CreateIndexOptions { Name = "ix_reviewable_order_items_shop_id" })
        ];

        await context.ReviewableOrderItems.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
