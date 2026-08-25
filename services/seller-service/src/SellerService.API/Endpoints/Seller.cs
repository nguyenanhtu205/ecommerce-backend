using MediatR;
using SellerService.Application.Features.Commands.ActivateShop;
using SellerService.Application.Features.Commands.ConnectShippingCarrier;
using SellerService.Application.Features.Commands.CreateShop;
using SellerService.Application.Features.Commands.CreateShopQuickReply;
using SellerService.Application.Features.Commands.UpdateShopBasicInformation;
using SellerService.Application.Features.Commands.UpdateShopChatSetting;
using SellerService.Application.Features.Commands.UpdateShopQuickReply;
using SellerService.Application.Features.Commands.UpdateShopVacationSetting;
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

        groupBuilder.MapGet(GetShopBasicInformation, "shop/basic-information")
            .Produces<GetShopBasicInformationResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(GetShopsInfoForChat, "shop/chat-information")
            .Produces<List<GetShopsInfoForChatItem>>()
            .RequireRateLimiting("post");

        groupBuilder.MapPatch(UpdateShopBasicInformation, "shop/basic-information")
            .RequireRateLimiting("put");

        groupBuilder.MapGet(GetShopInformation, "shop/information")
            .Produces<GetShopInformationResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetShopInformationForBuyer, "shop/public-information/{shopId}")
            .Produces<GetShopInformationForBuyerResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(GetShopsShippingInformation, "shop/shipping-information")
            .Produces<List<GetShopShippingInfoItem>>()
            .RequireRateLimiting("post");

        groupBuilder.MapGet(GetShopShippingConnections, "shop/shipping-connections")
            .Produces<List<CarrierItem>>()
            .RequireRateLimiting("get");

        groupBuilder.MapGet(GetShopVacationSetting, "shop/vacation-setting")
            .Produces<GetShopVacationSettingResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPatch(UpdateShopVacationSetting, "shop/vacation-setting")
            .RequireRateLimiting("put");

        groupBuilder.MapGet(GetShopChatSetting, "shop/chat-setting")
            .Produces<GetShopChatSettingResponse>()
            .RequireRateLimiting("get");

        groupBuilder.MapPut(UpdateShopChatSetting, "shop/chat-setting")
            .RequireRateLimiting("put");

        groupBuilder.MapGet(GetQuickReplies, "shop/chat-quick-replies")
            .Produces<List<QuickReply>>()
            .RequireRateLimiting("get");

        groupBuilder.MapPost(CreateQuickReply, "shop/chat-quick-reply")
            .RequireRateLimiting("post");

        groupBuilder.MapPut(UpdateQuickReply, "shop/chat-quick-reply")
            .RequireRateLimiting("put");
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

    [EndpointSummary("Get shop basic information")]
    public static async Task<IResult> GetShopBasicInformation(ISender sender, CancellationToken cancellationToken)
    {
        GetShopBasicInformationResponse result =
            await sender.Send(new GetShopBasicInformationQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shops information for chat")]
    public static async Task<IResult> GetShopsInfoForChat(GetShopsInfoForChatQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        List<GetShopsInfoForChatItem> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }


    [EndpointSummary("Update shop basic information")]
    public static async Task<IResult> UpdateShopBasicInformation(UpdateShopBasicInformationCommand command,
        ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Get shop information")]
    public static async Task<IResult> GetShopInformation(ISender sender, CancellationToken cancellationToken)
    {
        GetShopInformationResponse result = await sender.Send(new GetShopInformationQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop information for buyer")]
    public static async Task<IResult> GetShopInformationForBuyer(string shopId, ISender sender,
        CancellationToken cancellationToken)
    {
        GetShopInformationForBuyerResponse result =
            await sender.Send(new GetShopInformationForBuyerQuery(shopId), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop shipping information")]
    public static async Task<IResult> GetShopsShippingInformation(GetShopShippingInfoQuery query, ISender sender,
        CancellationToken cancellationToken)
    {
        List<GetShopShippingInfoItem> result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop shipping connection")]
    public static async Task<IResult> GetShopShippingConnections(ISender sender, CancellationToken cancellationToken)
    {
        List<CarrierItem> result = await sender.Send(new GetShippingCarrierConnectionsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Get shop vacation setting")]
    public static async Task<IResult> GetShopVacationSetting(ISender sender, CancellationToken cancellationToken)
    {
        GetShopVacationSettingResponse result = await sender.Send(new GetShopVacationSettingQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Update shop vacation setting")]
    public static async Task<IResult> UpdateShopVacationSetting(UpdateShopVacationSettingCommand command,
        ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Get shop chat setting")]
    public static async Task<IResult> GetShopChatSetting(ISender sender, CancellationToken cancellationToken)
    {
        GetShopChatSettingResponse result = await sender.Send(new GetShopChatSettingQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Update shop chat setting")]
    public static async Task<IResult> UpdateShopChatSetting(UpdateShopChatSettingCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Get quick replies")]
    public static async Task<IResult> GetQuickReplies(ISender sender, CancellationToken cancellationToken)
    {
        List<QuickReply> result = await sender.Send(new GetShopChatQuickReplyQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Create quick reply")]
    public static async Task<IResult> CreateQuickReply(CreateShopQuickReplyCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Update quick reply")]
    public static async Task<IResult> UpdateQuickReply(UpdateShopQuickReplyCommand command, ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
