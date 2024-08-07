using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Serilog;
using Serilog.Events;

namespace Melodee;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Log.Logger = new LoggerConfiguration()
            //.Filter.ByIncludingOnly(Matching.WithProperty("Area", LogArea.Control))
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollOnFileSizeLimit: true)            
            .CreateLogger();
  
        IconProvider.Current
            .Register<MaterialDesignIconProvider>();        
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}