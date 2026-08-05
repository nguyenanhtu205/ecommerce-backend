namespace OrderService.Infrastructure.Data.Configurations;

public class OrderReservationSagaStateConfiguration : IEntityTypeConfiguration<OrderReservationSagaState>
{
    public void Configure(EntityTypeBuilder<OrderReservationSagaState> builder)
    {
        builder.ToTable("order_reservation_saga_states");

        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CorrelationId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CurrentState)
            .HasColumnName("current_state")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CheckoutBatchId)
            .HasColumnName("checkout_batch_id")
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.PickupAddressSnapshot)
            .HasColumnName("pickup_address_snapshot")
            .HasColumnType("jsonb");

        builder.Property(x => x.DeliveryAddressSnapshot)
            .HasColumnName("delivery_address_snapshot")
            .HasColumnType("jsonb");

        builder.Property(x => x.Items)
            .HasColumnName("items")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<OrderReadyItem>>(v, (JsonSerializerOptions?)null) ??
                     new List<OrderReadyItem>())
            .Metadata.SetValueComparer(new ValueComparer<List<OrderReadyItem>>(
                (a, b) => (a ?? new List<OrderReadyItem>()).SequenceEqual(b ?? new List<OrderReadyItem>()),
                v => v.Aggregate(0, (hash, i) => HashCode.Combine(hash, i.GetHashCode())),
                v => v.ToList()));

        builder.Property(x => x.FailReason)
            .HasColumnName("fail_reason")
            .HasColumnType("text");

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.CheckoutBatchId);
    }
}
