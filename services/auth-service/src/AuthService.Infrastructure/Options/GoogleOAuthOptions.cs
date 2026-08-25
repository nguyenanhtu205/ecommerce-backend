namespace AuthService.Infrastructure.Options;

public class GoogleOAuthOptions
{
    public const string SectionName = "GoogleOAuth";

    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }

    public required string RedirectUri { get; set; }

    public string Scope { get; set; } = "openid email profile";
}
