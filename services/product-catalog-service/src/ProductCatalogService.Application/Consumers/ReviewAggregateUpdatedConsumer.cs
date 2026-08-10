namespace ProductCatalogService.Application.Consumers;

public class ReviewAggregateUpdatedConsumer(
    IApplicationDbContext dbContext,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer) : IConsumer<ReviewAggregateUpdated>
{
    public async Task Consume(ConsumeContext<ReviewAggregateUpdated> context)
    {
        ReviewAggregateUpdated message = context.Message;

        FilterDefinition<ProductListingView> filter = Builders<ProductListingView>.Filter.And(
            Builders<ProductListingView>.Filter.Eq(v => v.Id, message.ProductId),
            Builders<ProductListingView>.Filter.Lt(v => v.SyncedAt, message.UpdatedAt));

        UpdateDefinition<ProductListingView> update = Builders<ProductListingView>.Update
            .Set(v => v.RatingAverage, message.RatingAverage)
            .Set(v => v.RatingCount, message.RatingCount)
            .Set(v => v.SyncedAt, message.UpdatedAt);

        ProductListingView? updatedView = await dbContext.ProductListingViews.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<ProductListingView> { ReturnDocument = ReturnDocument.After },
            context.CancellationToken);

        if (updatedView is null)
        {
            return;
        }

        await listingViewProducer.Produce(new ProductListingViewUpdated(
            updatedView.Id,
            updatedView.ShopId,
            updatedView.ShopName,
            updatedView.Name,
            updatedView.Description,
            updatedView.Brand,
            updatedView.Tags,
            updatedView.SearchableSpecs,
            updatedView.ThumbnailUrl,
            updatedView.Location,
            [.. updatedView.CategoryPath.Select(c => new CategoryPathItemEvent(c.Id, c.Name))],
            updatedView.PriceMin,
            updatedView.PriceMax,
            updatedView.OriginalPriceMin,
            updatedView.DiscountPercent,
            updatedView.StockTotal,
            updatedView.IsOutOfStock,
            updatedView.RatingAverage,
            updatedView.RatingCount,
            updatedView.SoldCount,
            updatedView.SyncedAt), context.CancellationToken);
    }
}
