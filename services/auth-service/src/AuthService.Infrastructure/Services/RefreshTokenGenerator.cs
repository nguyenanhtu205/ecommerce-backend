namespace AuthService.Infrastructure.Services;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
