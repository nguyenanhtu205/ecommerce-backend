namespace ProductCatalogService.Infrastructure.Data.Configurations;

public class CategoryAttributesIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        CreateIndexModel<CategoryAttribute> index = new(
            Builders<CategoryAttribute>.IndexKeys.Ascending(x => x.CategoryId),
            new CreateIndexOptions { Name = "ix_category_attributes_category_id" });

        await context.CategoryAttributes.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
    }
}
