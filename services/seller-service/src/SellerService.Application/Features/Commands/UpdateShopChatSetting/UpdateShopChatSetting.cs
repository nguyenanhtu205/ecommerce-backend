namespace SellerService.Application.Features.Commands.UpdateShopChatSetting;

public record UpdateShopChatSettingCommand(bool AutoReplyEnabled, string AutoReplyMessage)
    : IRequest;

public class UpdateShopChatSetting(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<ShopChatSettingUpdated> producer)
    : IRequestHandler<UpdateShopChatSettingCommand>
{
    public async Task Handle(UpdateShopChatSettingCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ShopChatSetting? chatSetting = await context.ShopChatSettings
            .Where(s => s.ShopId == currentUser.ShopId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (chatSetting is null)
        {
            ShopChatSetting newChatSetting = new()
            {
                ShopId = currentUser.ShopId.Value,
                AutoReplyEnabled = request.AutoReplyEnabled,
                AutoReplyMessage = request.AutoReplyMessage,
                AwayModeEnabled = false
            };

            context.ShopChatSettings.Add(newChatSetting);
        }
        else
        {
            chatSetting.AutoReplyEnabled = request.AutoReplyEnabled;
            chatSetting.AutoReplyMessage = request.AutoReplyMessage;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (request.AutoReplyEnabled)
        {
            await producer.Produce(new ShopChatSettingUpdated(currentUser.ShopId.Value.ToString(),
                request.AutoReplyEnabled, request.AutoReplyMessage), cancellationToken);
        }
    }
}
