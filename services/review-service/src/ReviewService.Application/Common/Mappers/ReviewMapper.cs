namespace ReviewService.Application.Common.Mappers;

public static class ReviewMapper
{
    public static ReviewDto ToDto(Review review, bool isLikedByCurrentUser)
    {
        return new ReviewDto(
            review.Id,
            review.OrderItemId,
            review.ProductId,
            review.ShopId,
            review.BuyerId,
            review.BuyerDisplayName,
            review.Rating,
            review.Variation,
            [.. review.Attributes.Select(a => new ReviewAttributeDto(a.Label, a.Value))],
            review.Comment,
            review.MediaAssetIds,
            review.LikeCount,
            review.CreatedAt,
            review.SellerReply,
            isLikedByCurrentUser);
    }
}
