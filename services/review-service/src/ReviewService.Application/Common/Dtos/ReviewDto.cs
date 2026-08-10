namespace ReviewService.Application.Common.Dtos;

public record ReviewDto(
    string Id,
    string OrderItemId,
    string ProductId,
    string ShopId,
    string BuyerId,
    string BuyerDisplayName,
    int Rating,
    string? Variation,
    List<ReviewAttributeDto> Attributes,
    string Comment,
    List<string> MediaAssetIds,
    int LikeCount,
    DateTimeOffset CreatedAt);
