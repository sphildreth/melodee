using System;
using System.Net.Http;
using System.Reflection;
using Melodee;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Discovery.Releases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;
using Serilog;


namespace Melodee
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
            
            appBuilder.Services.AddLogging();

            appBuilder.RootComponents.Add<App>("app");

            var appDir = new DirectoryInfo(Assembly.GetEntryAssembly()!.Location);
            var configuration = System.Text.Json.JsonSerializer.Deserialize<Configuration>(File.ReadAllText(Path.Combine(appDir.Parent!.FullName, "configuration.json"))) ?? new Common.Models.Configuration.Configuration();
            appBuilder.Services.AddSingleton(configuration);

            appBuilder.Services.AddSerilog();
            
            appBuilder.Services.AddSingleton<IReleasesDiscoverer, ReleasesDiscoverer>();            
            
            var app = appBuilder.Build();

            app.MainWindow
                .SetIconFile("favicon.ico")
                .SetTitle("Melodee");

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
            };

            app.Run();

        }
    }
}
