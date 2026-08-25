namespace ReviewService.Application.Features.Queries;

public record ReviewCountsDto(
    long All,
    long ToReply,
    long Replied,
    Dictionary<int, long> Stars);

public record GetShopReviewCountsQuery : IRequest<ReviewCountsDto>;

public class GetShopReviewCounts(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<GetShopReviewCountsQuery, ReviewCountsDto>
{
    public async Task<ReviewCountsDto> Handle(GetShopReviewCountsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string shopId = currentUser.ShopId.Value.ToString();
        FilterDefinitionBuilder<Review> builder = Builders<Review>.Filter;
        FilterDefinition<Review> byShop = builder.Eq(r => r.ShopId, shopId);

        Task<long> allTask = context.Reviews.CountDocumentsAsync(byShop, cancellationToken: cancellationToken);
        Task<long> toReplyTask = context.Reviews.CountDocumentsAsync(
            byShop & builder.Eq(r => r.SellerReply, null), cancellationToken: cancellationToken);
        Task<long> repliedTask = context.Reviews.CountDocumentsAsync(
            byShop & builder.Ne(r => r.SellerReply, null), cancellationToken: cancellationToken);

        Task<long>[] starTasks =
        [
            .. Enumerable.Range(1, 5).Select(star =>
                context.Reviews.CountDocumentsAsync(
                    byShop & builder.Eq(r => r.Rating, star), cancellationToken: cancellationToken))
        ];

        await Task.WhenAll([allTask, toReplyTask, repliedTask, .. starTasks]);

        Dictionary<int, long> stars = Enumerable.Range(1, 5)
            .Select((star, idx) => (star, count: starTasks[idx].Result))
            .ToDictionary(x => x.star, x => x.count);

        return new ReviewCountsDto(allTask.Result, toReplyTask.Result, repliedTask.Result, stars);
    }
}
