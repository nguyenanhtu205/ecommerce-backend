namespace ReviewService.Application.Common.Mappers;

public static class ShopReviewMapper
{
    public static ShopReviewDto ToDto(Review review)
    {
        return new ShopReviewDto(
            review.Id,
            review.OrderItemId,
            review.ProductId,
            review.BuyerId,
            review.BuyerDisplayName,
            review.Rating,
            review.Variation,
            [.. review.Attributes.Select(a => new ShopReviewAttributeDto(a.Label, a.Value))],
            review.Comment,
            review.MediaAssetIds ?? [],
            review.LikeCount,
            review.CreatedAt,
            review.SellerReply is null
                ? null
                : new SellerReplyDto(review.SellerReply.Content, review.SellerReply.RepliedAt));
    }
}
