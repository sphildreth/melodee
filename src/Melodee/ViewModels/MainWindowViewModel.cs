using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DynamicData;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
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
    
    public ObservableCollection<ReleaseGrid> ReleaseGridInfos { get; set; } = [];
    
    public WriteableBitmap ReleasePrimaryCoverImage { get; set; }
   
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
    
    public MainWindowViewModel(Configuration configuration, IReleasesDiscoverer releasesDiscoverer)
    {
        ReleasesDiscoverer = releasesDiscoverer;
       
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
}