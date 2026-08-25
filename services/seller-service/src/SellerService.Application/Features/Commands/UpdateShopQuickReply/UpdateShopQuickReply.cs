namespace SellerService.Application.Features.Commands.UpdateShopQuickReply;

public record UpdateShopQuickReplyCommand(Guid Id, string Title, string Content) : IRequest;

public class UpdateShopQuickReply(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<UpdateShopQuickReplyCommand>
{
    public async Task Handle(UpdateShopQuickReplyCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ShopChatQuickReply? reply = await context.ShopChatQuickReplies
            .FindAsync([request.Id], cancellationToken);

        if (reply is null)
        {
            throw new NotFoundException("Reply not found");
        }

        if (reply.ShopId != currentUser.ShopId.Value)
        {
            throw new ForbiddenAccessException();
        }

        reply.Title = request.Title;
        reply.Content = request.Content;

        await context.SaveChangesAsync(cancellationToken);
    }
}
