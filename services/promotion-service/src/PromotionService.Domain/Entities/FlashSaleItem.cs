namespace PromotionService.Domain.Entities;

public class FlashSaleItem : BaseEntity
{
    public required Guid CampaignId { get; init; }

    public required Guid CombinationId { get; init; }

    public required int DiscountedPrice { get; init; }

    public int? QuantityLimit { get; init; }

    public required int QuantitySold { get; init; }

    public FlashSaleCampaign? FlashSaleCampaign { get; init; }
}
