using MediatR;
using SellerService.Application.Features.Commands.ActivateShop;
using SellerService.Application.Features.Commands.ConnectShippingCarrier;
using SellerService.Application.Features.Commands.CreateShop;
using SellerService.Application.Features.Queries.GetShopInformation;

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

        groupBuilder.MapPost(ConnectShippingCarrier, "shop/shipping-carrier/connect")
            .Produces<ConnectShippingCarrierResult>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetShopInformation, "shop/information")
            .Produces<GetShopInformationResponse>()
            .RequireRateLimiting("get");
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

    [EndpointSummary("Connect shipping carrier")]
    public static async Task<IResult> ConnectShippingCarrier(ConnectShippingCarrierCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        ConnectShippingCarrierResult result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop information")]
    public static async Task<IResult> GetShopInformation(ISender sender, CancellationToken cancellationToken)
    {
        GetShopInformationResponse result = await sender.Send(new GetShopInformationQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
