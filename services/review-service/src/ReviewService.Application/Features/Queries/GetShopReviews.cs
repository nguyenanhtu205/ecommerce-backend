namespace ReviewService.Application.Features.Queries;

public enum ReviewStatusFilter
{
    All,
    ToReply,
    Replied
}

public record GetShopReviewsQuery(
    List<int>? Ratings,
    ReviewStatusFilter Status = ReviewStatusFilter.All,
    int Page = 1,
    int PageSize = 20) : IRequest<List<ShopReviewDto>>;

public class GetShopReviews(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<GetShopReviewsQuery, List<ShopReviewDto>>
{
    public async Task<List<ShopReviewDto>> Handle(GetShopReviewsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string shopId = currentUser.ShopId.Value.ToString();

        FilterDefinitionBuilder<Review> builder = Builders<Review>.Filter;
        FilterDefinition<Review> filter = builder.Eq(r => r.ShopId, shopId);

        if (request.Ratings is { Count: > 0 })
        {
            filter &= builder.In(r => r.Rating, request.Ratings);
        }

        filter &= request.Status switch
        {
            ReviewStatusFilter.ToReply => builder.Eq(r => r.SellerReply, null),
            ReviewStatusFilter.Replied => builder.Ne(r => r.SellerReply, null),
            _ => builder.Empty
        };

        List<Review> reviews = await context.Reviews
            .Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        return [.. reviews.Select(ShopReviewMapper.ToDto)];
    }
}
