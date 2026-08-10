namespace ReviewService.Application.Features.Queries;

public record GetReviewAggregateByProductQuery(string ProductId) : IRequest<ReviewAggregateDto>;

public class GetReviewAggregateByProduct(IApplicationDbContext context)
    : IRequestHandler<GetReviewAggregateByProductQuery, ReviewAggregateDto>
{
    public async Task<ReviewAggregateDto> Handle(
        GetReviewAggregateByProductQuery request, CancellationToken cancellationToken)
    {
        ReviewAggregate? aggregate = await context.ReviewAggregates
            .Find(a => a.Id == request.ProductId)
            .FirstOrDefaultAsync(cancellationToken);

        if (aggregate is null)
        {
            return new ReviewAggregateDto(
                request.ProductId, 0, 0,
                new Dictionary<string, int>
                {
                    ["1"] = 0,
                    ["2"] = 0,
                    ["3"] = 0,
                    ["4"] = 0,
                    ["5"] = 0
                },
                0, 0, DateTimeOffset.UtcNow);
        }

        return new ReviewAggregateDto(
            aggregate.Id, aggregate.RatingAverage, aggregate.RatingCount, aggregate.StarCounts,
            aggregate.CommentCount, aggregate.MediaCount, aggregate.UpdatedAt);
    }
}
