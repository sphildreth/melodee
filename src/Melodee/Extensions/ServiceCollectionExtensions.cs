using Melodee.Plugins.Discovery.Directory;
using Melodee.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Melodee.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<IDirectoryDiscoverer, DirectoryDiscoverer>();
        collection.AddTransient<MainWindowViewModel>();
    }    
}