using MediatR;
using ReviewService.Application.Common.Dtos;
using ReviewService.Application.Features.Commands.CreateReview;
using ReviewService.Application.Features.Commands.DeleteReview;
using ReviewService.Application.Features.Commands.LikeReview;
using ReviewService.Application.Features.Queries;

namespace ReviewService.API.Endpoints;

public class Review : IEndpointGroup
{
    public static string RoutePrefix => "/review";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateReview, "reviews")
            .Produces<ReviewDto>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(LikeReview, "reviews/{id}/like")
            .RequireRateLimiting("post");

        groupBuilder.MapDelete(DeleteReview, "reviews/{id}")
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetReviewsByProduct, "products/{productId}/reviews")
            .Produces<List<ReviewDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetReviewAggregateByProduct, "products/{productId}/reviews/aggregate")
            .Produces<ReviewAggregateDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetPendingReviews, "reviews/pending")
            .Produces<List<ReviewableOrderItemDto>>()
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

    [EndpointSummary("Delete review")]
    [EndpointDescription("Used by seller/admin to remove a review that violates policy.")]
    public static async Task<IResult> DeleteReview(
        string id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteReviewCommand(id), cancellationToken);
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
}
