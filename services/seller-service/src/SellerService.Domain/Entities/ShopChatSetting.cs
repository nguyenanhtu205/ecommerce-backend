namespace SellerService.Domain.Entities;

public class ShopChatSetting
{
    public required Guid ShopId { get; init; }

    public required bool AutoReplyEnabled { get; set; }

    public string? AutoReplyMessage { get; set; }

    public required bool AwayModeEnabled { get; set; }

    public Shop? Shop { get; init; }
}
