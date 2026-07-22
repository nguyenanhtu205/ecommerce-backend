namespace AuthService.Application.Features.Commands.RefreshUserToken;

public record RefreshUserTokenResponse(string AccessToken, string RefreshToken);

public record RefreshUserTokenCommand(string RefreshToken) : IRequest<RefreshUserTokenResponse>;

public class RefreshUserToken(
    IApplicationDbContext context,
    IJwtProvider jwtProvider,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher
) : IRequestHandler<RefreshUserTokenCommand, RefreshUserTokenResponse>
{
    public async Task<RefreshUserTokenResponse> Handle(RefreshUserTokenCommand request,
        CancellationToken cancellationToken)
    {
        string tokenHash = refreshTokenHasher.Hash(request.RefreshToken);

        RefreshToken? existingToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            throw new NotFoundException("Invalid refresh token.");
        }

        if (existingToken.RevokedAt is not null)
        {
            await context.RefreshTokens
                .Where(rt => rt.UserId == existingToken.UserId && rt.RevokedAt == null)
                .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(rt => rt.RevokedAt, DateTimeOffset.UtcNow),
                    cancellationToken);

            throw new ForbiddenAccessException(
                "Refresh token has already been used. Please login again.");
        }

        if (existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ForbiddenAccessException("Refresh token has expired. Please login again.");
        }

        User? user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.Status == UserStatus.Banned)
        {
            throw new ForbiddenAccessException("Your account has been banned.");
        }

        if (user.Status == UserStatus.PendingVerification)
        {
            throw new ForbiddenAccessException("Please verify your email before continuing.");
        }

        existingToken.RevokedAt = DateTimeOffset.UtcNow;

        string newAccessToken = jwtProvider.Generate(user);
        string newRefreshToken = refreshTokenGenerator.Generate();
        string newRefreshTokenHash = refreshTokenHasher.Hash(newRefreshToken);

        RefreshToken newRefreshTokenEntity = new()
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.RefreshTokens.Add(newRefreshTokenEntity);

        await context.SaveChangesAsync(cancellationToken);

        return new RefreshUserTokenResponse(newAccessToken, newRefreshToken);
    }
}
