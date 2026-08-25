using System.Reflection;

namespace OrderService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<OrderReservationSagaState> OrderReservationSagaStates => Set<OrderReservationSagaState>();
    public DbSet<CheckoutSagaState> CheckoutSagaStates => Set<CheckoutSagaState>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();

    public DbSet<OrderShippingSnapshot> OrderShippingSnapshots => Set<OrderShippingSnapshot>();

    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
