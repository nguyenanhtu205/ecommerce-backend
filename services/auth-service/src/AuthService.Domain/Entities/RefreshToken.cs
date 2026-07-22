namespace AuthService.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required Guid UserId { get; init; }

    public required string TokenHash { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset? RevokedAt { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }

    public User? User { get; init; }
}
