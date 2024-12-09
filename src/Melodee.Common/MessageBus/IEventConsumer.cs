namespace Melodee.Common.MessageBus;

public interface IConsumer : IAsyncDisposable
{
    ValueTask Start(CancellationToken token = default);
    ValueTask Stop(CancellationToken token = default);
}

public interface IConsumer<T> : IConsumer
{
}
