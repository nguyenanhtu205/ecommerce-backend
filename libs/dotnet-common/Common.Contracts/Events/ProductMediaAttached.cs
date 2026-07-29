namespace Common.Contracts.Events;

public record ProductMediaAttached(
    string ProductId,
    string ShopId,
    List<MediaAttachmentItem> MediaAttachments,
    DateTimeOffset OccurredAt);

public record MediaAttachmentItem(
    string MediaAssetId,
    string Role, // 'thumbnail' | 'cover' | 'gallery' | 'avatar' | 'video' | ...
    int Position);
