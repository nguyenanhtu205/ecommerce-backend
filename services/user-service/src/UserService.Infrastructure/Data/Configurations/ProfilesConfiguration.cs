namespace UserService.Infrastructure.Data.Configurations;

public class ProfilesConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasColumnName("avatar_url");

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
