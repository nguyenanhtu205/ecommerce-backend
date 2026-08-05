using Ardalis.GuardClauses;
using Common.Contracts.Grpc.Inventory;
using Common.Contracts.Grpc.Promotion;
using Common.Contracts.Grpc.Shipping;
using Common.Contracts.Grpc.Shop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Services;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(connectionString, message: "Connection string not found.");

        NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
        dataSourceBuilder.EnableDynamicJson();

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(dataSource);
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services
            .AddGrpcClient<InventoryGrpcService.InventoryGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:InventoryGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:InventoryGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IInventoryServiceClient, InventoryServiceClient>();

        builder.Services
            .AddGrpcClient<ShopGrpcService.ShopGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:ShopGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:ShopGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IShopServiceClient, ShopServiceClient>();

        builder.Services
            .AddGrpcClient<PromotionGrpcService.PromotionGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:PromotionGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:PromotionGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IPromotionServiceClient, PromotionServiceClient>();

        builder.Services
            .AddGrpcClient<ShippingGrpcService.ShippingGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:ShippingGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:ShippingGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IShippingServiceClient, ShippingServiceClient>();
    }
}
