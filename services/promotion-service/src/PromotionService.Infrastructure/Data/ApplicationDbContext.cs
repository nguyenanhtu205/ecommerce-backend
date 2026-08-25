using System.Reflection;

namespace PromotionService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    public DbSet<Voucher> Vouchers => Set<Voucher>();

    public DbSet<VoucherRedemption> VoucherRedemptions => Set<VoucherRedemption>();

    public DbSet<FlashSaleCampaign> FlashSaleCampaigns => Set<FlashSaleCampaign>();

    public DbSet<FlashSaleItem> FlashSaleItems => Set<FlashSaleItem>();

    public DbSet<QuantityDiscount> QuantityDiscounts => Set<QuantityDiscount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
