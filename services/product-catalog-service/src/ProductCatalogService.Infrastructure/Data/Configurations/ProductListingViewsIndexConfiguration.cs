namespace ProductCatalogService.Infrastructure.Data.Configurations;

public class ProductListingViewsIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<ProductListingView>> indexes =
        [
            new(
                Builders<ProductListingView>.IndexKeys.Ascending("categoryPath.id"),
                new CreateIndexOptions { Name = "ix_listing_view_category_path_id" }),

            new(
                Builders<ProductListingView>.IndexKeys.Ascending(x => x.ShopId),
                new CreateIndexOptions { Name = "ix_listing_view_shop_id" }),

            new(
                Builders<ProductListingView>.IndexKeys.Ascending(x => x.PriceMin),
                new CreateIndexOptions { Name = "ix_listing_view_price_min" }),

            new(
                Builders<ProductListingView>.IndexKeys.Descending(x => x.SoldCount),
                new CreateIndexOptions { Name = "ix_listing_view_sold_count" }),

            new(
                Builders<ProductListingView>.IndexKeys.Descending(x => x.RatingAverage),
                new CreateIndexOptions { Name = "ix_listing_view_rating_average" }),

            new(
                Builders<ProductListingView>.IndexKeys.Text(x => x.Name).Text(x => x.Tags),
                new CreateIndexOptions { Name = "tx_listing_view_name_tags" }),

            new(
                Builders<ProductListingView>.IndexKeys
                    .Ascending(x => x.ShopId)
                    .Descending(x => x.SoldCount),
                new CreateIndexOptions { Name = "ix_listing_view_shop_id_sold_count" }),

            new(
                Builders<ProductListingView>.IndexKeys
                    .Ascending(x => x.ShopId)
                    .Descending(x => x.SyncedAt),
                new CreateIndexOptions { Name = "ix_listing_view_shop_id_synced_at" })
        ];

        await context.ProductListingViews.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
