namespace UserService.Application.Features.Queries.GetUsersForChat;

public record GetUsersForChatItem(string Id, string Name, string? UserAvatarUrl);

public record GetUsersForChatQuery(List<string> UserIds) : IRequest<List<GetUsersForChatItem>>;

public class GetUsersForChat(IApplicationDbContext context)
    : IRequestHandler<GetUsersForChatQuery, List<GetUsersForChatItem>>
{
    public async Task<List<GetUsersForChatItem>> Handle(GetUsersForChatQuery request,
        CancellationToken cancellationToken)
    {
        List<Guid> userIds =
        [
            .. request.UserIds
                .Select(id => Guid.TryParse(id, out Guid guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
        ];

        List<GetUsersForChatItem> users = await context.Profiles
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .Select(x => new GetUsersForChatItem(x.Id.ToString(), x.DisplayName, x.AvatarUrl))
            .ToListAsync(cancellationToken);

        Dictionary<string, GetUsersForChatItem> userDict = users.ToDictionary(x => x.Id);

        return
        [
            .. request.UserIds
                .Where(userDict.ContainsKey)
                .Select(id => userDict[id])
        ];
    }
}
