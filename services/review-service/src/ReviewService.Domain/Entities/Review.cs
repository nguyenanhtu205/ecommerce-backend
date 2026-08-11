namespace ReviewService.Domain.Entities;

public class Review
{
    public required string Id { get; init; }

    public required string OrderItemId { get; init; }

    public required string ProductId { get; init; }

    public required string ShopId { get; init; }

    public required string BuyerId { get; init; }

    public required string BuyerDisplayName { get; init; }

    public required int Rating { get; init; }

    public required string Variation { get; init; }

    public List<Attribute> Attributes { get; init; } = [];

    public required string Comment { get; init; }

    public List<string> MediaAssetIds { get; init; } = [];

    public required int LikeCount { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public Reply? SellerReply { get; set; }
}

public class Attribute
{
    public required string Label { get; init; }

    public required string Value { get; init; }
}

public class Reply
{
    public required string Content { get; init; }

    public required DateTimeOffset RepliedAt { get; init; }
}
