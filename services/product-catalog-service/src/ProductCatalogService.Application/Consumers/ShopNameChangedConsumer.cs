namespace ProductCatalogService.Application.Consumers;

public class ShopNameChangedConsumer(
    IApplicationDbContext dbContext,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer) : IConsumer<ShopNameChanged>
{
    private const int PublishConcurrency = 16;

    public async Task Consume(ConsumeContext<ShopNameChanged> context)
    {
        ShopNameChanged message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        FilterDefinition<ProductListingView> staleFilter = Builders<ProductListingView>.Filter.And(
            Builders<ProductListingView>.Filter.Eq(v => v.ShopId, message.ShopId),
            Builders<ProductListingView>.Filter.Lt(v => v.SyncedAt, message.ChangedAt));

        UpdateDefinition<ProductListingView> update = Builders<ProductListingView>.Update
            .Set(v => v.ShopName, message.ShopName)
            .Set(v => v.SyncedAt, message.ChangedAt);

        UpdateResult updateResult = await dbContext.ProductListingViews.UpdateManyAsync(
            staleFilter,
            update,
            cancellationToken: cancellationToken);

        if (updateResult.ModifiedCount == 0)
        {
            return;
        }

        FilterDefinition<ProductListingView> updatedFilter = Builders<ProductListingView>.Filter.And(
            Builders<ProductListingView>.Filter.Eq(v => v.ShopId, message.ShopId),
            Builders<ProductListingView>.Filter.Eq(v => v.SyncedAt, message.ChangedAt));

        using IAsyncCursor<ProductListingView> cursor = await dbContext.ProductListingViews.FindAsync(
            updatedFilter,
            cancellationToken: cancellationToken);

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = PublishConcurrency, CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(
            cursor.ToAsyncEnumerable(),
            parallelOptions,
            async (view, ct) =>
            {
                await listingViewProducer.Produce(new ProductListingViewUpdated(
                    view.Id,
                    view.ShopId,
                    view.ShopName,
                    view.Name,
                    view.Description,
                    view.Brand,
                    view.Tags,
                    view.SearchableSpecs,
                    view.ThumbnailUrl,
                    view.Location,
                    [.. view.CategoryPath.Select(c => new CategoryPathItemEvent(c.Id, c.Name))],
                    view.PriceMin,
                    view.PriceMax,
                    view.OriginalPriceMin,
                    view.DiscountPercent,
                    view.StockTotal,
                    view.IsOutOfStock,
                    view.RatingAverage,
                    view.RatingCount,
                    view.SoldCount,
                    view.SyncedAt), ct);
            });
    }
}
