namespace AuthService.Application.Common.Interfaces;

public interface IGoogleOAuthService
{
    string GetAuthorizationUrl(string state);

    Task<GoogleUserInfo> ExchangeCodeForUserInfoAsync(string code, CancellationToken cancellationToken);
}

public record GoogleUserInfo(string Email, string GoogleUserId, bool EmailVerified);
