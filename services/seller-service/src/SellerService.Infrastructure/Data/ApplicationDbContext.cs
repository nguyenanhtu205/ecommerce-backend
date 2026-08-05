using System.Reflection;
using MassTransit;

namespace SellerService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Shop> Shops => Set<Shop>();

    public DbSet<ShopBankAccount> ShopBankAccounts => Set<ShopBankAccount>();

    public DbSet<ShopChatQuickReply> ShopChatQuickReplies => Set<ShopChatQuickReply>();

    public DbSet<ShopChatSetting> ShopChatSettings => Set<ShopChatSetting>();

    public DbSet<ShopPaymentSetting> ShopPaymentSettings => Set<ShopPaymentSetting>();

    public DbSet<ShopShippingCarrierConnection> ShopShippingCarrierConnections => Set<ShopShippingCarrierConnection>();

    public DbSet<ShopVacationSetting> ShopVacationSettings => Set<ShopVacationSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
