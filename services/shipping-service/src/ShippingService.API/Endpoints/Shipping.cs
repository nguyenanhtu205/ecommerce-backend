using MediatR;
using ShippingService.Application.Features.Commands.CalculateShippingFee;
using ShippingService.Application.Features.Queries.GetCarriers;

namespace ShippingService.API.Endpoints;

public class Shipping : IEndpointGroup
{
    public static string RoutePrefix => "/shipping";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAllCarriers, "carriers")
            .Produces<List<GetCarriesQueryResponse>>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(CalculateShippingFee, "fee")
            .Produces<List<CalculateShippingFeeItemResult>>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Get all carriers")]
    public static async Task<IResult> GetAllCarriers(ISender sender, CancellationToken cancellationToken)
    {
        List<GetCarriesQueryResponse> result = await sender.Send(new GetCarriersQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Calculate shipping fee")]
    public static async Task<IResult> CalculateShippingFee(CalculateShippingFeeCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        List<CalculateShippingFeeItemResult> result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
