namespace AuthService.Domain.Entities;

public class AuthCredential : BaseEntity
{
    public required Guid UserId { get; init; }

    public required string PasswordHash { get; set; }

    public DateTimeOffset? PasswordChangedAt { get; set; }

    public User? User { get; init; }
}
