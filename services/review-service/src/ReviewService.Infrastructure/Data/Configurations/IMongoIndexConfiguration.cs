namespace ReviewService.Infrastructure.Data.Configurations;

public interface IMongoIndexConfiguration
{
    Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken);
}
