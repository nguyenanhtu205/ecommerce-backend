namespace ReviewService.Domain.Entities;

public class ReviewableOrderItem
{
    public required string Id { get; init; }

    public required string ProductId { get; init; }

    public required string ShopId { get; init; }

    public required string BuyerId { get; init; }

    public string? Variation { get; init; }

    public required bool IsReviewed { get; init; }

    public required DateTimeOffset OrderCompletedAt { get; init; }
}
