using System.Reflection;
using MassTransit;

namespace OrderService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<CheckoutSagaState> CheckoutSagaStates => Set<CheckoutSagaState>();

    public DbSet<OrderReservationSagaState> OrderReservationSagaStates => Set<OrderReservationSagaState>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();

    public DbSet<OrderShippingSnapshot> OrderShippingSnapshots => Set<OrderShippingSnapshot>();

    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
