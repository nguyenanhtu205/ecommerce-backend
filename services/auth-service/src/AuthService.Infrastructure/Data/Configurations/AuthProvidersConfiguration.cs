namespace AuthService.Infrastructure.Data.Configurations;

public class AuthProvidersConfiguration : IEntityTypeConfiguration<AuthProvider>
{
    public void Configure(EntityTypeBuilder<AuthProvider> builder)
    {
        builder.ToTable("auth_providers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.ProviderType)
            .HasColumnName("provider_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => new { x.ProviderType, x.ProviderUserId })
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.ProviderType })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuthProviders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
