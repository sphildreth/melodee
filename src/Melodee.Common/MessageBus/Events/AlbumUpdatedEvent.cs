namespace Melodee.Common.MessageBus.Events;

public sealed record AlbumUpdatedEvent(int? AlbumId, string? AlbumPath);
