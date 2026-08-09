namespace AuthService.Infrastructure.Data.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id");

        builder.Property(x => x.ShopName)
            .HasColumnName("shop_name")
            .HasMaxLength(255);

        builder.Property(x => x.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
