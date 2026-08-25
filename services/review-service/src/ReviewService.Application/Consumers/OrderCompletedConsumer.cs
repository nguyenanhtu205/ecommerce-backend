namespace ReviewService.Application.Consumers;

public class OrderCompletedConsumer(IApplicationDbContext dbContext) : IConsumer<OrderCompleted>
{
    public async Task Consume(ConsumeContext<OrderCompleted> context)
    {
        List<ReviewableOrderItem> documents =
        [
            .. context.Message.Items
                .Select(item => new ReviewableOrderItem
                {
                    Id = item.OrderItemId.ToString(),
                    ProductId = item.ProductId.ToString(),
                    ShopId = context.Message.ShopId.ToString(),
                    BuyerId = context.Message.BuyerId.ToString(),
                    Variation = item.Variation,
                    IsReviewed = false,
                    OrderCompletedAt = context.Message.CompletedAt.UtcDateTime
                })
        ];

        if (documents.Count == 0)
        {
            return;
        }

        IEnumerable<ReplaceOneModel<ReviewableOrderItem>> bulkOps = documents.Select(doc =>
            new ReplaceOneModel<ReviewableOrderItem>(
                Builders<ReviewableOrderItem>.Filter.Eq(
                    x => x.Id,
                    doc.Id
                ),
                doc) { IsUpsert = true });

        await dbContext.ReviewableOrderItems.BulkWriteAsync(bulkOps, cancellationToken: context.CancellationToken);
    }
}
