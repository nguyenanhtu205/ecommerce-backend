namespace PaymentService.Application.Common.Interfaces;

public interface IApplicationDbContext : IOutboxDbContext
{
    DbSet<Payment> Payments { get; }

    DbSet<Refund> Refunds { get; }

    DbSet<PaymentOrderLink> PaymentOrderLinks { get; }

    DbSet<EscrowHold> EscrowHolds { get; }

    DbSet<ShopWallet> ShopWallets { get; }

    DbSet<ShopWalletTransaction> ShopWalletTransactions { get; }
}
