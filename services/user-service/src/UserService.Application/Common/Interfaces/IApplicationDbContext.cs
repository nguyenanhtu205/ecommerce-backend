namespace UserService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Address> Addresses { get; }

    DbSet<Profile> Profiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
