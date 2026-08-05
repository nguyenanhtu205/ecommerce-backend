namespace ShippingService.Infrastructure.Data.Configurations;

public class PickupPointsConfiguration : IEntityTypeConfiguration<PickupPoint>
{
    public void Configure(EntityTypeBuilder<PickupPoint> builder)
    {
        builder.ToTable("pickup_points");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasOne(x => x.Carrier)
            .WithMany(x => x.PickupPoints)
            .HasForeignKey(x => x.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
