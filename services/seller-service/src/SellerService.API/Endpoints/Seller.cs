using MediatR;
using SellerService.Application.Features.Commands.ActivateShop;
using SellerService.Application.Features.Commands.CreateShop;

namespace SellerService.API.Endpoints;

public class Seller : IEndpointGroup
{
    public static string RoutePrefix => "/seller";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateShop, "shop")
            .Produces<CreateShopResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPost(ActivateShop, "shop/activate")
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Create shop")]
    public static async Task<IResult> CreateShop(CreateShopCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        CreateShopResponse result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activate shop")]
    public static async Task<IResult> ActivateShop(ActivateShopCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
