namespace ReviewService.Application.Common.Dtos;

public record ShopReviewAttributeDto(string Label, string Value);

public record SellerReplyDto(string Content, DateTimeOffset RepliedAt);

public record ShopReviewDto(
    string Id,
    string OrderItemId,
    string ProductId,
    string BuyerId,
    string BuyerDisplayName,
    int Rating,
    string? Variation,
    List<ShopReviewAttributeDto> Attributes,
    string Comment,
    List<string> MediaAssetIds,
    long LikeCount,
    DateTimeOffset CreatedAt,
    SellerReplyDto? SellerReply);
