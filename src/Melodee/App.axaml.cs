using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Extensions;
using Melodee.ViewModels;
using Melodee.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Melodee;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        var configuration =
            System.Text.Json.JsonSerializer.Deserialize<Configuration>(
                System.IO.File.ReadAllText("configuration.json"));
        collection.AddSingleton(configuration!);
        
        // TODO load plugin and see if configured to be enabled
        
        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();        
        
        var vm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(configuration)
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}