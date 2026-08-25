namespace PromotionService.Infrastructure.Data.Configurations;

public class ProcessedEventsConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.EventId)
            .IsUnique();

        builder.HasIndex(x => x.EventType);
    }
}
