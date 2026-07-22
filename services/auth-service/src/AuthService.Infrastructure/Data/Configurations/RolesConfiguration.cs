namespace AuthService.Infrastructure.Data.Configurations;

public class RolesConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly Guid BuyerRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid SellerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new Role { Id = BuyerRoleId, Name = "buyer" },
            new Role { Id = SellerRoleId, Name = "seller" }
        );
    }
}
