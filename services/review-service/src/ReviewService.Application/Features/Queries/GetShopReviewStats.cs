namespace ReviewService.Application.Features.Queries;

public record ShopReviewStatsDto(
    long TotalReviews,
    double TotalReviewsTrendPercent,
    double OrderReviewRate,
    double GoodReviewRate,
    long NeedReplyCount,
    double OverallRating);

public record GetShopReviewStatsQuery : IRequest<ShopReviewStatsDto>;

public class GetShopReviewStats(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<GetShopReviewStatsQuery, ShopReviewStatsDto>
{
    public async Task<ShopReviewStatsDto> Handle(GetShopReviewStatsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string shopId = currentUser.ShopId.Value.ToString();
        DateTime now = DateTime.UtcNow;
        DateTime periodStart = now.AddDays(-30);
        DateTime prevPeriodStart = now.AddDays(-60);

        FilterDefinitionBuilder<Review> rf = Builders<Review>.Filter;
        FilterDefinition<Review> byShop = rf.Eq(r => r.ShopId, shopId);

        Task<long> totalTask = context.Reviews.CountDocumentsAsync(byShop, cancellationToken: cancellationToken);

        Task<long> currentPeriodTask = context.Reviews.CountDocumentsAsync(
            byShop & rf.Gte(r => r.CreatedAt, periodStart), cancellationToken: cancellationToken);

        Task<long> prevPeriodTask = context.Reviews.CountDocumentsAsync(
            byShop & rf.Gte(r => r.CreatedAt, prevPeriodStart) & rf.Lt(r => r.CreatedAt, periodStart),
            cancellationToken: cancellationToken);

        Task<long> goodTask = context.Reviews.CountDocumentsAsync(
            byShop & rf.Gte(r => r.Rating, 4), cancellationToken: cancellationToken);

        Task<long> needReplyTask = context.Reviews.CountDocumentsAsync(
            byShop & rf.Lte(r => r.Rating, 2) & rf.Eq(r => r.SellerReply, null), cancellationToken: cancellationToken);

        FilterDefinitionBuilder<ReviewableOrderItem> of = Builders<ReviewableOrderItem>.Filter;
        FilterDefinition<ReviewableOrderItem> orderByShop = of.Eq(o => o.ShopId, shopId);

        Task<long> deliveredTask = context.ReviewableOrderItems.CountDocumentsAsync(
            orderByShop, cancellationToken: cancellationToken);

        Task<long> reviewedOrderTask = context.ReviewableOrderItems.CountDocumentsAsync(
            orderByShop & of.Eq(o => o.IsReviewed, true), cancellationToken: cancellationToken);

        await Task.WhenAll(totalTask, currentPeriodTask, prevPeriodTask, goodTask, needReplyTask, deliveredTask,
            reviewedOrderTask);

        double overallRating = totalTask.Result == 0
            ? 0
            : await context.Reviews.Aggregate()
                .Match(byShop)
                .Group(r => 1, g => new { Avg = g.Average(x => x.Rating) })
                .SingleOrDefaultAsync(cancellationToken)
                .ContinueWith(t => t.Result?.Avg ?? 0, cancellationToken);

        double trend = prevPeriodTask.Result == 0
            ? 0
            : Math.Round((currentPeriodTask.Result - prevPeriodTask.Result) * 100.0 / prevPeriodTask.Result, 1);

        double goodRate = totalTask.Result == 0 ? 0 : Math.Round(goodTask.Result * 100.0 / totalTask.Result, 1);
        double orderReviewRate = deliveredTask.Result == 0
            ? 0
            : Math.Round(reviewedOrderTask.Result * 100.0 / deliveredTask.Result, 1);

        return new ShopReviewStatsDto(
            totalTask.Result,
            trend,
            orderReviewRate,
            goodRate,
            needReplyTask.Result,
            Math.Round(overallRating, 1));
    }
}
