namespace Melodee.Common.MessageBus.Events;

public sealed record AlbumAddEvent(int ArtistId, string AlbumDirectory, bool IsFromArtistScan);
