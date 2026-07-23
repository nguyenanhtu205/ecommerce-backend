namespace SellerService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Shop> Shops { get; }

    DbSet<ShopBankAccount> ShopBankAccounts { get; }

    DbSet<ShopChatQuickReply> ShopChatQuickReplies { get; }

    DbSet<ShopChatSetting> ShopChatSettings { get; }

    DbSet<ShopPaymentSetting> ShopPaymentSettings { get; }

    DbSet<ShopShippingCarrierConnection> ShopShippingCarrierConnections { get; }

    DbSet<ShopVacationSetting> ShopVacationSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
