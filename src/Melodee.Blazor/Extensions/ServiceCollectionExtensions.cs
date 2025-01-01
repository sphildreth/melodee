using System.Threading.Channels;
using Melodee.Common.MessageBus;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Melodee.Blazor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryEvent<T, THandler>(this IServiceCollection services) where THandler : class, IEventHandler<T>
    {
        var bus = Channel.CreateUnbounded<Event<T>>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
            }
        );
        services.AddScoped<IEventHandler<T>, THandler>();
        services.AddSingleton(typeof(IEventPublisher<T>), _ => new InMemoryEventBusPublisher<T>(bus.Writer));
        var consumerFactory = (IServiceProvider provider) => new InMemoryEventBusConsumer<T>(
            bus.Reader,
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<Serilog.ILogger>()
        );
        services.AddSingleton(typeof(IConsumer), consumerFactory.Invoke);
        services.AddSingleton(typeof(IConsumer<T>), consumerFactory.Invoke);
        services.TryAddSingleton(typeof(IEventContextAccessor<>), typeof(EventContextAccessor<>));
        return services;
    }
    
    public static async Task<IServiceProvider> StartConsumersAsync(this IServiceProvider services)
    {
        var consumers = services.GetServices<IConsumer>();
        foreach (var consumer in consumers)
        {
            await consumer.Start().ConfigureAwait(false);
        }
        return services;
    }

    public static async Task<IServiceProvider> StopConsumersAsync(this IServiceProvider services)
    {
        var consumers = services.GetServices<IConsumer>();
        foreach (var consumer in consumers)
        {
            await consumer.Stop().ConfigureAwait(false);
        }
        return services;
    }    
}
