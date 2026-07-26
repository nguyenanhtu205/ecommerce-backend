namespace ProductCatalogService.Infrastructure.Data.Configurations;

public class CategoriesIndexConfiguration : IMongoIndexConfiguration
{
    public async Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<CreateIndexModel<Category>> indexes =
        [
            new(
                Builders<Category>.IndexKeys.Ascending(x => x.Slug),
                new CreateIndexOptions { Unique = true, Name = "ux_categories_slug" }),

            new(
                Builders<Category>.IndexKeys.Ascending(x => x.ParentId),
                new CreateIndexOptions { Name = "ix_categories_parent_id" })
        ];

        await context.Categories.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
