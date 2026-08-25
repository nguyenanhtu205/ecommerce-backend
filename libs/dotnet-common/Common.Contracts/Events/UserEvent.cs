namespace Common.Contracts.Events;

public record PickupAddressSnapshotUpdated(
    Guid UserId,
    string FullName,
    string Phone,
    string Province,
    string Ward,
    string AddressDetail,
    string FullAddressText,
    decimal? Latitude,
    decimal? Longitude,
    string AddressType
);

public record AvatarMediaAttached(
    string UserId,
    List<MediaAttachmentItem> MediaAttachments,
    DateTimeOffset OccurredAt);
