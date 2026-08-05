namespace PromotionService.Domain.Entities;

public class VoucherRedemption : BaseEntity
{
    public required Guid VoucherId { get; init; }

    public required Guid OrderId { get; init; }

    public required Guid UserId { get; init; }

    public required int DiscountAmount { get; init; }

    public required DateTimeOffset RedeemedAt { get; init; }

    public Voucher? Voucher { get; init; }
}
