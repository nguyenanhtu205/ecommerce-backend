namespace SellerService.Domain.Entities;

public class ShopChatQuickReply : BaseEntity
{
    public required Guid ShopId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public Shop? Shop { get; init; }
}
