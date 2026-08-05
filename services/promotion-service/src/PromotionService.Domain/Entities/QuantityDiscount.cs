namespace PromotionService.Domain.Entities;

public class QuantityDiscount : BaseEntity
{
    public required Guid ProductId { get; init; }

    public required int MinQuantity { get; init; }

    public required int DiscountPercent { get; init; }
}
