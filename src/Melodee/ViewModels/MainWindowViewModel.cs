using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using DynamicData;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Models;
using Melodee.Plugins.Discovery.Releases;
using ReactiveUI;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IReleasesDiscoverer ReleasesDiscoverer { get; }
    
    public Configuration Configuration { get; }
    
    public ObservableCollection<ReleaseGrid> ReleaseGridInfos { get; set; } = [];
    
    public WriteableBitmap? ReleasePrimaryCoverImage { get; set; }
   
    private bool _showSelectedRelease;

    public bool ShowSelectedRelease
    {
        get => _showSelectedRelease;
        set => this.RaiseAndSetIfChanged(ref _showSelectedRelease, value);
    }

    private ReleaseDetail? _releaseDetail;
    
    public ReleaseDetail? SelectedRelease
    {
        get => _releaseDetail;
        set
        {
            this.RaiseAndSetIfChanged(ref _releaseDetail, value);
            ShowSelectedRelease = _releaseDetail != null;            
        }
    }
    
    public bool IsLoading { get; set; }

    public bool HandleDeleteSelectedItems()
    {
        return false;
    }     
    
    public bool HandleDeleteRelease()
    {
        IsLoading = true;
        using (Operation.At(LogEventLevel.Debug).Time("Deleting Release [{SelectedRelease}]", SelectedRelease.ToString()))
        {
            var detailResult = Task.Run(() => ReleasesDiscoverer.ReleaseByUniqueIdAsync(Configuration.StagingDirectoryInfo, SelectedRelease.UniqueId)).Result;
            var deleted  = detailResult.Delete(Configuration.StagingDirectory);
            if (!deleted)
            {
                return false;
            }
            ReleaseGridInfos.Remove(ReleaseGridInfos.First(x => x.UniqueId == SelectedRelease.UniqueId));
            IsLoading = false;
            return true;
        }
    }
    
    private WindowBase? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        return null;
    }    
    
    public async Task<bool> HandleExploreRelease()
    {
        IsLoading = true;
        var detailResult = Task.Run(() => ReleasesDiscoverer.ReleaseByUniqueIdAsync(Configuration.StagingDirectoryInfo, SelectedRelease.UniqueId)).Result;
        var topLevel = GetTopLevel();
        if (topLevel != null)
        {
            var launcher = TopLevel.GetTopLevel(topLevel)?.Launcher;
            if (launcher != null)
            {
                IsLoading = false;
                var releaseDirInfo = new DirectoryInfo(Path.Combine(Configuration.StagingDirectory, detailResult.ToDirectoryName()));
              //  var releaseJsonInfo = new FileInfo(Path.Combine(releaseDirInfo.FullName, Release.JsonFileName));
              //  return await launcher.LaunchFileInfoAsync(releaseJsonInfo);
                return await launcher.LaunchDirectoryInfoAsync(releaseDirInfo);
            }
        }
        IsLoading = false;
        return true;
    }

    public MainWindowViewModel()
    {
        Configuration = new Configuration
        {
            InboundDirectory = string.Empty,
            StagingDirectory = string.Empty,
            LibraryDirectory = string.Empty,
            Scripting = new Scripting(),
            MediaConvertorOptions = new MediaConvertorOptions(),
            PluginProcessOptions = new PluginProcessOptions()
        };
        ReleasesDiscoverer = new ReleasesDiscoverer(Configuration);
    }

    public MainWindowViewModel(Configuration configuration, IReleasesDiscoverer releasesDiscoverer)
    {
        ReleasesDiscoverer = releasesDiscoverer;
        Configuration = configuration;
       
        IsLoading = true;
        SelectedRelease = null;
        using (Operation.At(LogEventLevel.Debug).Time("Discovering releases for [{File}]", configuration.StagingDirectoryInfo))
        {
            var pagedResult = Task.Run(() => releasesDiscoverer.ReleasesGridsForDirectoryAsync(configuration.StagingDirectoryInfo, new PagedRequest())).Result;
            ReleaseGridInfos.Clear();
            ReleaseGridInfos.AddRange(pagedResult.Data);
            IsLoading = false;
        }
    }
    
    public static WriteableBitmap CreateBitmapFromPixelData( 
        byte[] bgraPixelData, 
        int pixelWidth, 
        int pixelHeight) 
    { 
        // Standard may need to change on some devices 
        Vector dpi = new Vector(96, 96); 
  
        var bitmap = new WriteableBitmap( 
            new PixelSize(pixelWidth, pixelHeight), 
            dpi, 
            PixelFormat.Bgra8888, 
            AlphaFormat.Premul); 
  
        using (var frameBuffer = bitmap.Lock()) 
        { 
            Marshal.Copy(bgraPixelData, 0, frameBuffer.Address, bgraPixelData.Length); 
        } 
  
        return bitmap; 
    } 

    
}