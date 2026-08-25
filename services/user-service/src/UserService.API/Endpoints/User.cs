using MediatR;
using UserService.Application.Features.Commands.CreateAddress;
using UserService.Application.Features.Commands.DeleteAddress;
using UserService.Application.Features.Commands.SetDefaultAddress;
using UserService.Application.Features.Commands.UpdateAddress;
using UserService.Application.Features.Commands.UpdateProfile;
using UserService.Application.Features.Queries.GetAddress;
using UserService.Application.Features.Queries.GetProfile;
using UserService.Application.Features.Queries.GetUsersForChat;

namespace UserService.API.Endpoints;

public class User : IEndpointGroup
{
    public static string RoutePrefix => "/user";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProfile, "profile")
            .Produces<ProfileDto>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetAddresses, "addresses")
            .Produces<List<AddressDto>>()
            .RequireRateLimiting("get");

        groupBuilder.MapPatch(UpdateProfile, "profile")
            .RequireRateLimiting("put");

        groupBuilder.MapPost(CreateAddress, "address")
            .Produces<CreateAddressResponse>()
            .RequireRateLimiting("post");

        groupBuilder.MapPatch(UpdateAddress, "address")
            .RequireRateLimiting("put");

        groupBuilder.MapPatch(SetDefaultAddress, "address/{addressId}/default")
            .RequireRateLimiting("put");

        groupBuilder.MapDelete(DeleteAddress, "address/{addressId}")
            .RequireRateLimiting("delete");

        groupBuilder.MapPost(GetUsersForChat, "chat-information")
            .Produces<List<GetUsersForChatItem>>()
            .RequireRateLimiting("post");
    }

    [EndpointSummary("Get profile")]
    public static async Task<IResult> GetProfile(ISender sender, CancellationToken cancellationToken)
    {
        ProfileDto result = await sender.Send(new GetProfileQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get addresses of current user")]
    public static async Task<IResult> GetAddresses(ISender sender, CancellationToken cancellationToken)
    {
        List<AddressDto> result = await sender.Send(new GetAddressQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Update profile")]
    public static async Task<IResult> UpdateProfile(UpdateProfileCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Create address")]
    [EndpointDescription("Address Type: 0 (Home) or 1 (Office)")]
    public static async Task<IResult> CreateAddress(CreateAddressCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        CreateAddressResponse result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Update address")]
    public static async Task<IResult> UpdateAddress(UpdateAddressCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Set address to default")]
    public static async Task<IResult> SetDefaultAddress(string addressId, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SetDefaultAddressCommand(addressId), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Delete address")]
    public static async Task<IResult> DeleteAddress(string addressId, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteAddressCommand(addressId), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Get users information for chat")]
    public static async Task<IResult> GetUsersForChat(GetUsersForChatQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        List<GetUsersForChatItem> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
