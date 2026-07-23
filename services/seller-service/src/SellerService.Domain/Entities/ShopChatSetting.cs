namespace SellerService.Domain.Entities;

public class ShopChatSetting
{
    public required Guid ShopId { get; init; }

    public required bool AutoReplyEnabled { get; init; }

    public string? AutoReplyMessage { get; init; }

    public required bool AwayModeEnabled { get; init; }

    public Shop? Shop { get; init; }
}
