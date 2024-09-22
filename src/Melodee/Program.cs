using System.Reflection;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;
using Serilog;
using Toolbelt.Blazor.Extensions.DependencyInjection;


namespace Melodee;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
            
        appBuilder.Services.AddLogging();

        appBuilder.RootComponents.Add<App>("app");

        var appDir = new DirectoryInfo(Assembly.GetEntryAssembly()!.Location);
        appBuilder.Services.AddTransient(opt => System.Text.Json.JsonSerializer.Deserialize<Configuration>(File.ReadAllText(Path.Combine(appDir.Parent!.FullName, "configuration.json"))) ?? new Configuration());

        appBuilder.Services.AddSerilog();
            
        appBuilder.Services.AddSingleton<IReleasesDiscoverer, ReleasesDiscoverer>();
        appBuilder.Services.AddSingleton<IMetaTagsProcessorPlugin, MetaTagsProcessor>();
        appBuilder.Services.AddSingleton<ITrackPlugin, AtlMetaTag>();
        appBuilder.Services.AddSingleton<IReleaseValidator, ReleaseValidator>();
        appBuilder.Services.AddSingleton<IReleaseEditProcessor, ReleaseEditProcessor>();
        
        appBuilder.Services.AddTransient<IDirectoryProcessorPlugin, DirectoryProcessor>(opt =>
        {
            var configuration = opt.GetRequiredService<Configuration>();
            IScriptPlugin preScript = new NullScript(configuration);
            if (configuration.Scripting.PreDiscoveryScript != null)
            {
                preScript = new PreDiscoveryScript(configuration);
            }
            IScriptPlugin postScript = new NullScript(configuration);
            if (configuration.Scripting.PostDiscoveryScript != null)
            {
                postScript = new PostDiscoveryScript(configuration);
            }                
            return new DirectoryProcessor(preScript, postScript, opt.GetRequiredService<IReleaseValidator>(), configuration)
            {
                IsEnabled = false
            };  
        });
            
        appBuilder.Services.AddBlazorBootstrap();
            
        appBuilder.Services.AddHotKeys2();
            
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File("Logs/log.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",                    
                rollingInterval: RollingInterval.Day)
            .CreateLogger();            
            
        var app = appBuilder.Build();

        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetMinHeight(1024)
            .SetMinWidth(768)
            .SetTitle("Melodee");

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
        };

        app.Run();

    }
}
