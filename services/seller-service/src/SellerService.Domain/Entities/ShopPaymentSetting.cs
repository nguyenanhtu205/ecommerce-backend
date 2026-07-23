namespace SellerService.Domain.Entities;

public class ShopPaymentSetting
{
    public required Guid ShopId { get; init; }

    public required PayoutCycle PayoutCycle { get; init; } = PayoutCycle.Weekly;

    public Shop? Shop { get; init; }
}
