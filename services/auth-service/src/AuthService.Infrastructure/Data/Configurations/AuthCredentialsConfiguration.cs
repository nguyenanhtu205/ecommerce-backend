namespace AuthService.Infrastructure.Data.Configurations;

public class AuthCredentialsConfiguration : IEntityTypeConfiguration<AuthCredential>
{
    public void Configure(EntityTypeBuilder<AuthCredential> builder)
    {
        builder.ToTable("auth_credentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PasswordChangedAt)
            .HasColumnName("password_changed_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(x => x.AuthCredential)
            .HasForeignKey<AuthCredential>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
