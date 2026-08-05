using MediatR;
using OrderService.Application.Features.Commands.Checkout;

namespace OrderService.API.Endpoints;

public class Order : IEndpointGroup
{
    public static string RoutePrefix => "/order";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Checkout, "checkout")
            .Produces<CheckoutResult>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Checkout")]
    private static async Task<IResult> Checkout(CheckoutCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        CheckoutResult result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}
