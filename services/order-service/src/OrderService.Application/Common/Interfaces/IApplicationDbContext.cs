using OrderService.Application.Sagas.Checkout;

namespace OrderService.Application.Common.Interfaces;

public interface IApplicationDbContext : IOutboxDbContext
{
    DbSet<Order> Orders { get; }

    DbSet<OrderItem> OrderItems { get; }

    DbSet<OrderItemAddon> OrderItemAddons { get; }

    DbSet<OrderShippingSnapshot> OrderShippingSnapshots { get; }

    DbSet<OrderStatusHistory> OrderStatusHistories { get; }

    DbSet<OrderVoucher> OrderVouchers { get; }

    DbSet<CheckoutSagaState> CheckoutSagaStates { get; }
}
