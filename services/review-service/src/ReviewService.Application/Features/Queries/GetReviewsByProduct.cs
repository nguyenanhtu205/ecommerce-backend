namespace ReviewService.Application.Features.Queries;

public record GetReviewsByProductQuery(
    string ProductId,
    int? Rating,
    bool? HasComment,
    bool? HasMedia,
    int Page = 1,
    int PageSize = 20) : IRequest<List<ReviewDto>>;

public class GetReviewsByProduct(IApplicationDbContext context)
    : IRequestHandler<GetReviewsByProductQuery, List<ReviewDto>>
{
    public async Task<List<ReviewDto>> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<Review> builder = Builders<Review>.Filter;
        FilterDefinition<Review> filter = builder.Eq(r => r.ProductId, request.ProductId);

        if (request.Rating is not null)
        {
            filter &= builder.Eq(r => r.Rating, request.Rating.Value);
        }

        if (request.HasComment == true)
        {
            filter &= builder.Ne(r => r.Comment, string.Empty);
        }

        if (request.HasMedia == true)
        {
            filter &= builder.SizeGt(r => r.MediaAssetIds, 0);
        }

        List<Review> reviews = await context.Reviews
            .Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        return [.. reviews.Select(ReviewMapper.ToDto)];
    }
}
