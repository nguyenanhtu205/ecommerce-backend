namespace SellerService.Application.Features.Commands.CreateShopQuickReply;

public record CreateShopQuickReplyCommand(string Title, string Content) : IRequest;

public class CreateShopQuickReply(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<CreateShopQuickReplyCommand>
{
    public async Task Handle(CreateShopQuickReplyCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        context.ShopChatQuickReplies.Add(new ShopChatQuickReply
        {
            ShopId = currentUser.ShopId.Value, Title = request.Title, Content = request.Content
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
