using Melodee.Common.Models.Configuration;
using Melodee.Models;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Presentation;
using Melodee.Styles;
using Uno.Resizetizer;
using MainModel = Melodee.Presentation.MainModel;
using MainPage = Melodee.Presentation.MainPage;
using SecondModel = Melodee.Presentation.SecondModel;
using SecondPage = Melodee.Presentation.SecondPage;
using Shell = Melodee.Presentation.Shell;

namespace Melodee;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Load WinUI Resources
        Resources.Build(r => r.Merged(new XamlControlsResources()));

        // Load Uno.UI.Toolkit and Material Resources
        Resources.Build(r => r.Merged(
            new MaterialToolkitTheme(
                new ColorPaletteOverride(),
                new MaterialFontsOverride())));
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(
                    configure: (context, logBuilder) =>
                    {
                        logBuilder.SetMinimumLevel(context.HostingEnvironment.IsDevelopment()
                            ? LogLevel.Information
                            : LogLevel.Warning).CoreLogLevel(LogLevel.Warning);
                    }, enableUnoLogging: true)
                .UseSerilog(consoleLoggingEnabled: true, fileLoggingEnabled: true)
                .UseConfiguration(configure: configBuilder => configBuilder
                    .EmbeddedSource<App>()
                    .Section<Configuration>()
                )
                .UseLocalization()
                .UseSerialization((context, services) => services
                    .AddContentSerializer(context))
                .UseHttp()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IReleasesDiscoverer, ReleasesDiscoverer>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.EnableHotReload();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new("Main", View: views.FindByViewModel<MainModel>(), IsDefault: true),
                    new("Second", View: views.FindByViewModel<SecondModel>()),
                ]
            )
        );
    }
}
