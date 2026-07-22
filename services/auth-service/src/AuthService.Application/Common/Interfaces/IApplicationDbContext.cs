namespace AuthService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<AuthCredential> AuthCredentials { get; }

    DbSet<AuthProvider> AuthProviders { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Role> Roles { get; }

    DbSet<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
