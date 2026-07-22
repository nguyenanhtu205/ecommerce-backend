namespace UserService.Infrastructure.Data.Configurations;

public class AddressesConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Province)
            .HasColumnName("province")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Ward)
            .HasColumnName("ward")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.AddressDetail)
            .HasColumnName("address_detail")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FullAddressText)
            .HasColumnName("full_address_text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.AddressType)
            .HasColumnName("address_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsPickupAddress)
            .HasColumnName("is_pickup_address")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.Profile)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
