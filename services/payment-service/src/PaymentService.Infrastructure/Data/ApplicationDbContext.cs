using System.Reflection;

namespace PaymentService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<PaymentOrderLink> PaymentOrderLinks => Set<PaymentOrderLink>();

    public DbSet<EscrowHold> EscrowHolds => Set<EscrowHold>();

    public DbSet<ShopWallet> ShopWallets => Set<ShopWallet>();

    public DbSet<ShopWalletTransaction> ShopWalletTransactions => Set<ShopWalletTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
