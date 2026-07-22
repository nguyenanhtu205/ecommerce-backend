using System.Reflection;
using MassTransit;

namespace AuthService.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<AuthCredential> AuthCredentials => Set<AuthCredential>();

    public DbSet<AuthProvider> AuthProviders => Set<AuthProvider>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.AddInboxStateEntity();

        builder.AddOutboxMessageEntity();

        builder.AddOutboxStateEntity();
    }
}
