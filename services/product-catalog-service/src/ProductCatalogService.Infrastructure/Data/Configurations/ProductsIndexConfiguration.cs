namespace ProductCatalogService.Infrastructure.Data.Configurations;

public class ProductsIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<Product>> indexes =
        [
            new(
                Builders<Product>.IndexKeys.Ascending(x => x.ShopId).Ascending(x => x.Status),
                new CreateIndexOptions { Name = "ix_products_shop_id_status" }),

            new(
                Builders<Product>.IndexKeys.Ascending(x => x.CategoryId),
                new CreateIndexOptions { Name = "ix_products_category_id" }),

            new(
                Builders<Product>.IndexKeys.Ascending("variantCombinations.combinationId"),
                new CreateIndexOptions { Name = "ix_products_variant_combination_id" })
        ];

        await context.Products.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
