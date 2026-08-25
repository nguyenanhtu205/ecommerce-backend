namespace SellerService.Domain.Entities;

public class ShopChatQuickReply : BaseEntity
{
    public required Guid ShopId { get; init; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public Shop? Shop { get; init; }
}
