namespace PromotionService.Domain.Entities;

public class FlashSaleCampaign : BaseEntity
{
    public Guid? ShopId { get; init; }

    public required string Name { get; init; }

    public required CampaignStatus Status { get; init; }

    public required DateTimeOffset StartsAt { get; init; }

    public required DateTimeOffset EndsAt { get; init; }

    public ICollection<FlashSaleItem> FlashSaleItems { get; private set; } = new List<FlashSaleItem>();
}
