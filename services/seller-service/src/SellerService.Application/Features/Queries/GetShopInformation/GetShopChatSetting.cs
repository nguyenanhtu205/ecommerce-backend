namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopChatSettingResponse(bool AutoReplyEnabled, string AutoReplyMessage);

public record GetShopChatSettingQuery : IRequest<GetShopChatSettingResponse>;

public class GetShopChatSetting(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShopChatSettingQuery, GetShopChatSettingResponse>
{
    public async Task<GetShopChatSettingResponse> Handle(GetShopChatSettingQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ShopChatSetting? chatSetting = await context.ShopChatSettings
            .AsNoTracking()
            .Where(s => s.ShopId == currentUser.ShopId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return chatSetting is null
            ? new GetShopChatSettingResponse(false, "")
            : new GetShopChatSettingResponse(chatSetting.AutoReplyEnabled, chatSetting.AutoReplyMessage ?? "");
    }
}
