namespace PromotionService.Domain.Entities;

public class Voucher : BaseEntity
{
    public required string Code { get; init; }

    public required VoucherScope Scope { get; init; }

    public Guid? ShopId { get; init; }

    public int? DiscountAmount { get; init; }

    public int? DiscountPercent { get; init; }

    public required int MinOrderValue { get; init; }

    public int? MaxDiscountAmount { get; init; }

    public int? QuantityLimit { get; init; }

    public required int QuantityUsed { get; set; }

    public required DateTimeOffset StartsAt { get; init; }

    public required DateTimeOffset EndsAt { get; init; }

    public ICollection<VoucherRedemption> VoucherRedemptions { get; private set; } = new List<VoucherRedemption>();
}
