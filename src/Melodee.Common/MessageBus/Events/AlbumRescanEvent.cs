namespace Melodee.Common.MessageBus.Events;

public sealed record AlbumRescanEvent(int AlbumId, string AlbumDirectory);
