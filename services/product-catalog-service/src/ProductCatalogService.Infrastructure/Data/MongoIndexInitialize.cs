using System.Reflection;
using ProductCatalogService.Infrastructure.Data.Configurations;

namespace ProductCatalogService.Infrastructure.Data;

public class MongoIndexInitializer(IApplicationDbContext context)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<IMongoIndexConfiguration> configurations = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        typeof(IMongoIndexConfiguration).IsAssignableFrom(t))
            .Select(t => (IMongoIndexConfiguration)Activator.CreateInstance(t)!);

        foreach (IMongoIndexConfiguration configuration in configurations)
        {
            await configuration.ApplyAsync(context, cancellationToken);
        }
    }
}
