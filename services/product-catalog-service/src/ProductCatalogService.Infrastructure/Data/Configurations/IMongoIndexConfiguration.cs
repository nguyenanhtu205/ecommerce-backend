namespace ProductCatalogService.Infrastructure.Data.Configurations;

public interface IMongoIndexConfiguration
{
    Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken);
}
