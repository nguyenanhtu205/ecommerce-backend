using Ardalis.GuardClauses;
using Common.Contracts.Grpc.Inventory;
using Common.Contracts.Grpc.Product;
using Common.Contracts.Grpc.Promotion;
using Common.Contracts.Grpc.Shipping;
using Common.Contracts.Grpc.Shop;
using Common.Contracts.Grpc.User;
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

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dataSource));
        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

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


        builder.Services
            .AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:UserGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:UserGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

        builder.Services
            .AddGrpcClient<ProductGrpcService.ProductGrpcServiceClient>(o =>
            {
                o.Address = new Uri(builder.Configuration["Services:ProductGrpc"]
                                    ?? throw new InvalidOperationException("Missing Services:ProductGrpc config"));
            })
            .AddStandardResilienceHandler();

        builder.Services.AddScoped<IProductServiceClient, ProductServiceClient>();

        builder.Services.AddScoped<IOutboxWriter, OutboxWriter>();

        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<RedeemVoucher>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<CreatePayment>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderPaymentSucceeded>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderPaymentFailed>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ReleaseVoucher>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ReleaseStockCommand>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<CancelOrder>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ReserveStock>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderStockReserved>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderStockReservationFailed>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<CreateShipment>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<CheckoutInitiated>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<ReserveOrderStock>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<OrderCompleted>>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher<CommitStockCommand>>();

        builder.Services.AddHostedService<OutboxDispatcherBackgroundService<ApplicationDbContext>>();
    }
}
