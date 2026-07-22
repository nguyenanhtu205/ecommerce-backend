namespace AuthService.Application.Features.Commands.Logout;

public record LogoutCommand : IRequest;

public class Logout(
    IApplicationDbContext context,
    ICurrentUser currentUser
) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new ForbiddenAccessException("User was not logged in.");
        }

        await context.RefreshTokens
            .Where(rt => rt.UserId == currentUser.UserId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(rt => rt.RevokedAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }
}
