using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using ShippingService.Infrastructure.Carriers;
using ShippingService.Infrastructure.Data;

namespace ShippingService.Infrastructure;

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

        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ShipmentCreated>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ShipmentCreationFailed>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderDelivered>>();

        builder.Services.AddHostedService<OutboxDispatcherBackgroundService<ApplicationDbContext>>();

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.Configure<GhnOptions>(builder.Configuration.GetSection("Ghn"));
        builder.Services.Configure<GhtkOptions>(builder.Configuration.GetSection("Ghtk"));

        builder.Services.AddScoped<ICarrierShippingAdapter, MockCarrierAdapter>();

        builder.Services.AddHttpClient<GhnCarrierAdapter>((provider, client) =>
        {
            GhnOptions options = provider.GetRequiredService<IOptions<GhnOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        builder.Services.AddScoped<ICarrierShippingAdapter>(provider =>
            provider.GetRequiredService<GhnCarrierAdapter>());

        builder.Services.AddHttpClient<GhtkCarrierAdapter>((provider, client) =>
        {
            GhtkOptions options = provider.GetRequiredService<IOptions<GhtkOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        builder.Services.AddScoped<ICarrierShippingAdapter>(provider =>
            provider.GetRequiredService<GhtkCarrierAdapter>());

        builder.Services.AddScoped<ICarrierAdapterFactory, CarrierAdapterFactory>();


        builder.Services.AddHostedService<MockShipmentProgressionService>();
    }
}
