using InventoryService.Application.Features.Queries.GetInventoryForSeller;

namespace InventoryService.API.Endpoints;

public class Inventory : IEndpointGroup
{
    public static string RoutePrefix => "/inventory";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetInventoryForSeller, "stocks/shop/me")
            .Produces<List<GetInventoryForSellerResponse>>()
            .RequireRateLimiting("get");
    }

    [EndpointSummary("Get inventory for seller")]
    public static async Task<IResult> GetInventoryForSeller(ISender sender, CancellationToken cancellationToken)
    {
        List<GetInventoryForSellerResponse> result =
            await sender.Send(new GetInventoryForSellerQuery(), cancellationToken);
        return Results.Ok(result);
    }
}
