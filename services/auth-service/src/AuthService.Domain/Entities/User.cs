namespace AuthService.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; init; }

    public Guid? ShopId { get; set; }

    public string? ShopName { get; set; }

    public DateTimeOffset? EmailVerifiedAt { get; init; }

    public required UserStatus Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public AuthCredential? AuthCredential { get; init; }

    public ICollection<AuthProvider> AuthProviders { get; private set; } = new List<AuthProvider>();

    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
}
