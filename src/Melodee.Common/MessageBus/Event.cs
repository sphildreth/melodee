namespace Melodee.Common.MessageBus;

public record Event<T>(T? Data, EventMetadata? Metadata = default);
