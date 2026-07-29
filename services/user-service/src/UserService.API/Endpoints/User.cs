using MediatR;
using UserService.Application.Features.Commands.CreateAddress;

namespace UserService.API.Endpoints;

public class User : IEndpointGroup
{
    public static string RoutePrefix => "/user";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateAddress, "address")
            .Produces<CreateAddressResponse>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Create address")]
    [EndpointDescription("Address Type: 0 (Home) or 1 (Office)")]
    public static async Task<IResult> CreateAddress(CreateAddressCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        CreateAddressResponse result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
