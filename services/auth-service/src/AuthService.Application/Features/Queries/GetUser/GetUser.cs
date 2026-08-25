namespace AuthService.Application.Features.Queries.GetUser;

public record GetUserResponse(string Email, string? ShopName, DateTimeOffset CreatedAt);

public record GetUserQuery : IRequest<GetUserResponse>;

public class GetUser(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        User? user = await context.Users
            .AsNoTracking()
            .Where(u => u.ShopId == currentUser.ShopId.Value)
            .FirstOrDefaultAsync(cancellationToken);


        return user is null
            ? throw new NotFoundException("User not found")
            : new GetUserResponse(user.Email, user.ShopName, user.CreatedAt);
    }
}
