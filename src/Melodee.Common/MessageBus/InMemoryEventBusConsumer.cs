using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Melodee.Common.MessageBus;

public sealed class InMemoryEventBusConsumer<T>(ChannelReader<Event<T>> bus, IServiceScopeFactory scopeFactory, ILogger logger) 
    : IConsumer<T>
{
    private CancellationToken _stoppingToken;
    
    public async ValueTask Start(CancellationToken token = default)
    {
        _stoppingToken = token;

        await using var scope = scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>().ToList();
        var metadataAccessor = scope.ServiceProvider.GetRequiredService<IEventContextAccessor<T>>();
        if (handlers.FirstOrDefault() is null)
        {
            logger.Debug("No handlers defined for event of {type}", typeof(T).Name);
            return;
        }
        await Task.Run(
            async () => await StartProcessing(handlers, metadataAccessor).ConfigureAwait(false),
            token
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribes to channel changes and triggers event handling
    /// </summary>
    public async ValueTask StartProcessing(List<IEventHandler<T>> handlers, IEventContextAccessor<T> contextAccessor)
    {
        var continuousChannelIterator = bus.ReadAllAsync(_stoppingToken)
            .ConfigureAwait(false);

        await foreach (var task in continuousChannelIterator)
        {
            if (_stoppingToken.IsCancellationRequested)
                break;

            // invoke handlers in parallel
            await Parallel.ForEachAsync(handlers, _stoppingToken,
                async (handler, scopedToken) => await ExecuteHandler(handler, task, contextAccessor, scopedToken)
                    .ConfigureAwait(false)
            ).ConfigureAwait(false);
        }
    }    
    
    public ValueTask ExecuteHandler(IEventHandler<T> handler, Event<T> task, IEventContextAccessor<T> ctx, CancellationToken token)
    {
        ctx.Set(task); // set metadata and begin scope
       // using var logScope = _logger.BeginScope(task.Metadata ?? new EventMetadata(Guid.NewGuid().ToString()));

        Task.Run(
            async () => await handler.Handle(task.Data, token), token
        ).ConfigureAwait(false);

        return ValueTask.CompletedTask;
    }    
    
    public async ValueTask Stop(CancellationToken _ = default)
    {
        await DisposeAsync().ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }
}
