namespace UserService.Domain.Entities;

public class Profile
{
    public required Guid Id { get; init; }

    public required string DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public Gender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Address> Addresses { get; private set; } = new List<Address>();
}
