namespace AuthService.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    
    public required string Audience { get; init; }
    
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    
    public required string PrivateKeyPath { get; init; }
    
    public required string KeyId { get; init; }
}
