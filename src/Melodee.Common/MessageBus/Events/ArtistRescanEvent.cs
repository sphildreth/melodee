namespace Melodee.Common.MessageBus.Events;

public sealed record ArtistRescanEvent(int ArtistId, string ArtistDirectory);
