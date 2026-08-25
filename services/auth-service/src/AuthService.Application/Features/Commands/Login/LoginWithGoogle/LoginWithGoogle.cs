namespace AuthService.Application.Features.Commands.Login.LoginWithGoogle;

public record LoginWithGoogleResponse(string AccessToken, string RefreshToken);

public record LoginWithGoogleCommand(string AuthorizationCode, string State, string Role)
    : IRequest<LoginWithGoogleResponse>;

public class LoginWithGoogle(
    IApplicationDbContext context,
    IGoogleOAuthService googleOAuthService,
    IOAuthStateStore stateStore,
    IJwtProvider jwtProvider,
    ITopicProducer<UserRegistered> producer,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher
) : IRequestHandler<LoginWithGoogleCommand, LoginWithGoogleResponse>
{
    public async Task<LoginWithGoogleResponse> Handle(LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        bool isStateValid = await stateStore.ConsumeStateAsync(request.State, cancellationToken);

        if (!isStateValid)
        {
            throw new ForbiddenAccessException("Invalid or expired OAuth state.");
        }

        GoogleUserInfo googleUserInfo =
            await googleOAuthService.ExchangeCodeForUserInfoAsync(request.AuthorizationCode, cancellationToken);

        AuthProvider? existingProvider = await context.AuthProviders
            .Include(p => p.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                p => p.ProviderType == ProviderType.Google && p.ProviderUserId == googleUserInfo.GoogleUserId,
                cancellationToken);

        User user;

        if (existingProvider is not null)
        {
            user = existingProvider.User!;
        }
        else
        {
            User? existingUserByEmail = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == googleUserInfo.Email.Trim(), cancellationToken);

            if (existingUserByEmail is not null)
            {
                user = existingUserByEmail;

                context.AuthProviders.Add(new AuthProvider
                {
                    UserId = user.Id,
                    ProviderType = ProviderType.Google,
                    ProviderUserId = googleUserInfo.GoogleUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                Role? role = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);

                if (role is null)
                {
                    throw new NotFoundException("Role not found.");
                }

                User newUser = new()
                {
                    Email = googleUserInfo.Email.Trim(),
                    Status = UserStatus.Active,
                    EmailVerifiedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                
                await producer.Produce(
                    new UserRegistered(newUser.Id, newUser.Email, DateTimeOffset.UtcNow), cancellationToken);

                newUser.UserRoles.Add(new UserRole { UserId = newUser.Id, RoleId = role.Id, Role = role });

                context.Users.Add(newUser);

                context.AuthProviders.Add(new AuthProvider
                {
                    UserId = newUser.Id,
                    ProviderType = ProviderType.Google,
                    ProviderUserId = googleUserInfo.GoogleUserId,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                user = newUser;
            }
        }

        if (user.Status == UserStatus.Banned)
        {
            throw new ForbiddenAccessException("Your account has been banned.");
        }

        if (user.Status == UserStatus.PendingVerification)
        {
            throw new ForbiddenAccessException("Please verify your email before logging in.");
        }

        string accessToken = jwtProvider.Generate(user);
        string refreshToken = refreshTokenGenerator.Generate();
        string refreshTokenHash = refreshTokenHasher.Hash(refreshToken);

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return new LoginWithGoogleResponse(accessToken, refreshToken);
    }
}
