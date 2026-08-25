using Ardalis.GuardClauses;
using InventoryService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(connectionString, message: "Connection string not found.");

        NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
        dataSourceBuilder.EnableDynamicJson();

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dataSource));
        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        
        builder.Services.AddScoped<IOutboxWriter, OutboxWriter>();

        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<StockReserved>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<StockReservationFailed>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<StockCommited>>();
        
        builder.Services.AddHostedService<OutboxDispatcherBackgroundService<ApplicationDbContext>>();
        
        builder.Services.AddSingleton(TimeProvider.System);
    }
}
