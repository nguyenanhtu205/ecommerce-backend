namespace AuthService.Domain.Entities;

public class UserRole
{
    public required Guid UserId { get; init; }

    public required Guid RoleId { get; init; }

    public User? User { get; init; }

    public Role? Role { get; init; }
}
