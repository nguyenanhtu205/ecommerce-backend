namespace AuthService.Application.Features.Commands.Login.LoginWithPassword;

public record LoginWithPasswordResponse(string AccessToken, string RefreshToken);

public record LoginWithPasswordCommand(string Email, string Password) : IRequest<LoginWithPasswordResponse>;

public class LoginWithPassword(
    IApplicationDbContext context,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher
) : IRequestHandler<LoginWithPasswordCommand, LoginWithPasswordResponse>
{
    public async Task<LoginWithPasswordResponse> Handle(LoginWithPasswordCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim(), cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Invalid email or password.");
        }

        if (user.Status == UserStatus.Banned)
        {
            throw new ForbiddenAccessException("Your account has been banned.");
        }

        if (user.Status == UserStatus.PendingVerification)
        {
            throw new ForbiddenAccessException("Please verify your email before logging in.");
        }

        AuthCredential? credentials = await context.AuthCredentials
            .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

        if (credentials is null)
        {
            throw new NotFoundException("Invalid email or password.");
        }

        bool isPasswordValid = passwordHasher.Verify(request.Password, credentials.PasswordHash);

        if (!isPasswordValid)
        {
            throw new NotFoundException("Invalid email or password.");
        }

        string accessToken = jwtProvider.Generate(user);
        string refreshToken = refreshTokenGenerator.Generate();
        string refreshTokenHash = refreshTokenHasher.Hash(refreshToken);

        RefreshToken refreshTokenEntity = new()
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.RefreshTokens.Add(refreshTokenEntity);

        await context.SaveChangesAsync(cancellationToken);

        return new LoginWithPasswordResponse(accessToken, refreshToken);
    }
}
