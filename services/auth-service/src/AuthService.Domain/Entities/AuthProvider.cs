namespace AuthService.Domain.Entities;

public class AuthProvider : BaseEntity
{
    public required Guid UserId { get; init; }

    public required ProviderType ProviderType { get; init; }

    public required string ProviderUserId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public User? User { get; init; }
}
