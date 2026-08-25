namespace SellerService.Application.Features.Queries.GetShopInformation;

public record QuickReply(Guid Id, string Title, string Content);

public record GetShopChatQuickReplyQuery : IRequest<List<QuickReply>>;

public class GetShopChatQuickReply(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShopChatQuickReplyQuery, List<QuickReply>>
{
    public async Task<List<QuickReply>> Handle(GetShopChatQuickReplyQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<ShopChatQuickReply> replies = await context.ShopChatQuickReplies
            .AsNoTracking()
            .Where(s => s.ShopId == currentUser.ShopId.Value)
            .ToListAsync(cancellationToken);

        return [.. replies.Select(r => new QuickReply(r.Id, r.Title, r.Content))];
    }
}
