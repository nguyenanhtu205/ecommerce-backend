using MediatR;
using OrderService.Application.Features.Commands.Checkout;
using OrderService.Application.Features.Queries.CheckoutStatus;
using OrderService.Application.Features.Queries.GetAllOrderByUser;
using OrderService.Application.Features.Queries.GetOrderById;
using OrderService.Application.Features.Queries.GetOrderItemInfo;
using OrderService.Application.Features.Queries.GetOrdersForSeller;

namespace OrderService.API.Endpoints;

public class Order : IEndpointGroup
{
    public static string RoutePrefix => "/order";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Checkout, "checkout")
            .Produces<CheckoutResult>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetCheckoutStatus, "checkout/{checkoutBatchId}/status")
            .Produces<CheckoutBatchStatusResult?>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetAllOrderByUser, "user")
            .Produces<List<GetAllOrderByUserResponse>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetOrderById, "{orderId}")
            .Produces<GetOrderByIdResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetOrdersForSeller, "seller")
            .Produces<List<GetOrderForSellerResponse>>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(GetOrderItemInfo, "item-info")
            .Produces<List<OrderItemInfo>>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Checkout")]
    private static async Task<IResult> Checkout(CheckoutCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        CheckoutResult result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }

    [EndpointSummary("Get checkout status")]
    private static async Task<IResult> GetCheckoutStatus(Guid checkoutBatchId, ISender sender,
        CancellationToken cancellationToken)
    {
        CheckoutBatchStatusResult? result =
            await sender.Send(new GetCheckoutBatchStatusQuery(checkoutBatchId), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get all order by user")]
    private static async Task<IResult> GetAllOrderByUser(ISender sender, CancellationToken cancellationToken)
    {
        List<GetAllOrderByUserResponse> result = await sender.Send(new GetAllOrderByUserQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get order by id")]
    private static async Task<IResult> GetOrderById(Guid orderId, ISender sender, CancellationToken cancellationToken)
    {
        GetOrderByIdResponse result = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get orders for seller")]
    private static async Task<IResult> GetOrdersForSeller(ISender sender, CancellationToken cancellationToken)
    {
        List<GetOrderForSellerResponse> result = await sender.Send(new GetOrdersForSellerQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get order item info")]
    private static async Task<IResult> GetOrderItemInfo(GetOrderItemInfoQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        List<OrderItemInfo> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
