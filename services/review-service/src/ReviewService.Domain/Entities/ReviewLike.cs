namespace ReviewService.Domain.Entities;

public class ReviewLike
{
    public required string Id { get; init; }

    public required string ReviewId { get; init; }

    public required string BuyerId { get; init; }

    public required DateTimeOffset LikedAt { get; init; }
}
