namespace AuthService.Domain.Entities;

public class Role
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
}
