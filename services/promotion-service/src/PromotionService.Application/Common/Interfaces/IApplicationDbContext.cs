using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PromotionService.Application.Common.Interfaces;

public interface IApplicationDbContext : IOutboxDbContext
{
    DbSet<Voucher> Vouchers { get; }

    DbSet<VoucherRedemption> VoucherRedemptions { get; }

    DbSet<FlashSaleCampaign> FlashSaleCampaigns { get; }

    DbSet<FlashSaleItem> FlashSaleItems { get; }

    DbSet<QuantityDiscount> QuantityDiscounts { get; }

    ChangeTracker ChangeTracker { get; }
}
