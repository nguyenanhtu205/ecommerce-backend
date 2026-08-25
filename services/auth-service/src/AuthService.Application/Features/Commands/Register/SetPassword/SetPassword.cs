using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace AuthService.Application.Features.Commands.Register.SetPassword;

public record SetPasswordCommand(string Email, string Password) : IRequest<SetPasswordResponse>;

public record SetPasswordResponse(string AccessToken, string RefreshToken);

public class SetPasswordCommandHandler(
    IApplicationDbContext context,
    IOtpStore otpStore,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    ITopicProducer<UserRegistered> producer,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher) : IRequestHandler<SetPasswordCommand, SetPasswordResponse>
{
    public async Task<SetPasswordResponse> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        string? role = await otpStore.GetVerifiedRoleAsync(request.Email, cancellationToken);
        if (role is null)
        {
            throw new ValidationException(
                [new ValidationFailure("Email", "The email address has not been verified.")]);
        }

        User? existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        User user;
        bool isNewRegistration = existingUser is null;

        if (existingUser is not null)
        {
            if (existingUser.Status == UserStatus.Banned)
            {
                throw new ForbiddenAccessException("Your account has been banned.");
            }

            if (existingUser.Status == UserStatus.PendingVerification)
            {
                throw new ForbiddenAccessException("Please verify your email before logging in.");
            }

            user = existingUser;
        }
        else
        {
            Role roleEntity = await context.Roles.SingleAsync(r => r.Name == role, cancellationToken);

            user = new User
            {
                Email = request.Email,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            UserRole userRole = new() { UserId = user.Id, RoleId = roleEntity.Id };

            context.Users.Add(user);
            context.UserRoles.Add(userRole);
        }

        AuthCredential credential = new() { UserId = user.Id, PasswordHash = passwordHasher.Hash(request.Password) };
        context.AuthCredentials.Add(credential);

        if (isNewRegistration)
        {
            await producer.Produce(
                new UserRegistered(user.Id, user.Email, DateTimeOffset.UtcNow), cancellationToken);
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

        await otpStore.ClearVerifiedAsync(request.Email, cancellationToken);

        return new SetPasswordResponse(accessToken, refreshToken);
    }
}
