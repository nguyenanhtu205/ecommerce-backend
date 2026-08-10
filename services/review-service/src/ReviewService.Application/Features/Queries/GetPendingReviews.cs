namespace ReviewService.Application.Features.Queries;

public record GetPendingReviewsQuery : IRequest<List<ReviewableOrderItemDto>>;

public class GetPendingReviews(ICurrentUser currentUser, IApplicationDbContext context)
    : IRequestHandler<GetPendingReviewsQuery, List<ReviewableOrderItemDto>>
{
    public async Task<List<ReviewableOrderItemDto>> Handle(GetPendingReviewsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        string buyerId = currentUser.UserId.Value.ToString();

        List<ReviewableOrderItem> items = await context.ReviewableOrderItems
            .Find(x => x.BuyerId == buyerId && !x.IsReviewed)
            .SortByDescending(x => x.OrderCompletedAt)
            .ToListAsync(cancellationToken);

        return
        [
            .. items.Select(x => new ReviewableOrderItemDto(
                x.Id, x.ProductId, x.ShopId, x.Variation, x.IsReviewed, x.OrderCompletedAt))
        ];
    }
}
