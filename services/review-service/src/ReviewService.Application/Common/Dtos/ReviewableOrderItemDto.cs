namespace ReviewService.Application.Common.Dtos;

public record ReviewableOrderItemDto(
    string OrderItemId,
    string ProductId,
    string ShopId,
    string? Variation,
    bool IsReviewed,
    DateTimeOffset OrderCompletedAt);
