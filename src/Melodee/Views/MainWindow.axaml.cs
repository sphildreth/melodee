using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using Melodee.Common.Models;
using Melodee.ViewModels;
using SerilogTimings;

namespace Melodee.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationTree_OnTapped(object? sender, TappedEventArgs e)
    {
        if (!((e.Source as TextBlock)?.Tag is long clickedDirectoryId))
        {
            return;
        }
        var clickedDirectory =
            ((sender as TreeView)!.DataContext as MainWindowViewModel)!.InboundDirectoryInfos
            .First(x => x.DirectoryInfo.UniqueId == (long?)clickedDirectoryId);
        var dc = ((sender as TreeView)!.DataContext as MainWindowViewModel)!;
        var releasesDiscoverer = dc.ReleasesDiscoverer;
        dc.IsLoading = true;
        using (Operation.Time("Discovering releases for [{File}]", clickedDirectory.DirectoryInfo))
        {
            var pagedResult = Task.Run(() => releasesDiscoverer.ReleasesForDirectoryAsync(clickedDirectory.DirectoryInfo, new PagedRequest())).Result;
            dc.ReleaseInfos.Clear();
            dc.ReleaseInfos.AddRange(pagedResult.Data);
            dc.IsLoading = false;
        }
    }
}