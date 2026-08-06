namespace ReviewService.Infrastructure.Data.Configurations;

public class ReviewAggregatesIndexConfiguration : IMongoIndexConfiguration
{
    public Task ApplyAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
