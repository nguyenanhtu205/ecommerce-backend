using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalogService.Infrastructure.Caching;
using ProductCatalogService.Infrastructure.Data;

namespace ProductCatalogService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        MongoConventions.Register();
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(connectionString, message: "Connection string not found.");

        MongoUrl mongoUrl = MongoUrl.Create(connectionString);
        Guard.Against.NullOrEmpty(mongoUrl.DatabaseName, message: "Database name not found in connection string.");

        MongoClient client = new(mongoUrl);
        IMongoDatabase database = client.GetDatabase(mongoUrl.DatabaseName);

        builder.Services.AddSingleton(database);

        builder.Services.AddScoped<ApplicationDbContext>();
        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<MongoIndexInitializer>();

        builder.Services.AddSingleton(TimeProvider.System);

        string? redisConnectionString = builder.Configuration["Redis:ConnectionString"];
        Guard.Against.NullOrEmpty(redisConnectionString, message: "Redis connection string not found.");
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    }
}
