using Melodee.Plugins.Discovery.Releases;
using Melodee.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Melodee.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<IReleasesDiscoverer, ReleasesDiscoverer>();
        collection.AddTransient<MainWindowViewModel>();
    }    
}