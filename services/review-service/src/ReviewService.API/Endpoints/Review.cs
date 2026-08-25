using MediatR;
using ReviewService.Application.Common.Dtos;
using ReviewService.Application.Features.Commands.CreateReview;
using ReviewService.Application.Features.Commands.DeleteReview;
using ReviewService.Application.Features.Commands.LikeReview;
using ReviewService.Application.Features.Commands.ReplyToReview;
using ReviewService.Application.Features.Commands.UnlikeReview;
using ReviewService.Application.Features.Queries;

namespace ReviewService.API.Endpoints;

public class Review : IEndpointGroup
{
    public static string RoutePrefix => "/review";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateReview)
            .Produces<ReviewDto>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(LikeReview, "{id}/like")
            .RequireRateLimiting("post");

        groupBuilder.MapDelete(UnlikeReview, "{id}/like")
            .RequireRateLimiting("post");

        groupBuilder.MapDelete(DeleteReview, "{id}")
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetReviewsByProduct, "products/{productId}")
            .Produces<List<ReviewDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetReviewAggregateByProduct, "products/{productId}/aggregate")
            .Produces<ReviewAggregateDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetPendingReviews, "pending")
            .Produces<List<ReviewableOrderItemDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapPatch(ReplyToReview, "{id}/reply")
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetShopReviewCounts, "shop/counts")
            .Produces<ReviewCountsDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost("shop", GetShopReviews)
            .Produces<List<ShopReviewDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetShopReviewStats, "shop/stats")
            .Produces<ShopReviewStatsDto>()
            .RequireRateLimiting("get");
    }

    [EndpointSummary("Create review")]
    [EndpointDescription(
        "Buyer creates a review for an order item they purchased. orderItemId must exist in reviewable_order_items and not yet be reviewed.")]
    public static async Task<IResult> CreateReview(
        CreateReviewCommand command, ISender sender, CancellationToken cancellationToken)
    {
        ReviewDto result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Like review")]
    public static async Task<IResult> LikeReview(
        string id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new LikeReviewCommand(id), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Unlike review")]
    public static async Task<IResult> UnlikeReview(
        string id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new UnlikeReviewCommand(id), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Delete review")]
    [EndpointDescription("Used by seller/admin to remove a review that violates policy.")]
    public static async Task<IResult> DeleteReview(
        string id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteReviewCommand(id), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Reply to review")]
    [EndpointDescription("Seller replies to a review on their own product. A review can only be replied to once.")]
    public static async Task<IResult> ReplyToReview(
        string id, ReplyToReviewCommand command, ISender sender, CancellationToken cancellationToken)
    {
        if (id != command.ReviewId)
        {
            return Results.BadRequest("The ID in the route does not match the ID in the request body.");
        }

        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Get reviews by product")]
    [EndpointDescription("Buyer-facing list, supports filtering by rating/comment/media and pagination.")]
    public static async Task<IResult> GetReviewsByProduct(
        string productId, [AsParameters] GetReviewsByProductQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        if (productId != query.ProductId)
        {
            return Results.BadRequest("The productId in the route does not match the productId in the query.");
        }

        List<ReviewDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get review aggregate by product")]
    [EndpointDescription("Returns rating average/count and star breakdown, backed by review_aggregates.")]
    public static async Task<IResult> GetReviewAggregateByProduct(
        string productId, ISender sender, CancellationToken cancellationToken)
    {
        ReviewAggregateDto result =
            await sender.Send(new GetReviewAggregateByProductQuery(productId), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get pending reviews")]
    [EndpointDescription("Purchased order items of the current buyer that have not been reviewed yet.")]
    public static async Task<IResult> GetPendingReviews(
        ISender sender, CancellationToken cancellationToken)
    {
        List<ReviewableOrderItemDto> result = await sender.Send(new GetPendingReviewsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop review counts")]
    public static async Task<IResult> GetShopReviewCounts(ISender sender, CancellationToken cancellationToken)
    {
        ReviewCountsDto result = await sender.Send(new GetShopReviewCountsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop reviews")]
    public static async Task<IResult> GetShopReviews(GetShopReviewsQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        List<ShopReviewDto> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop review stats")]
    public static async Task<IResult> GetShopReviewStats(ISender sender, CancellationToken cancellationToken)
    {
        ShopReviewStatsDto result = await sender.Send(new GetShopReviewStatsQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
