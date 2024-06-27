using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Models;
using Melodee.ViewModels;
using Serilog;
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

        var allDirectoryInfos = ((sender as TreeView)!.DataContext as MainWindowViewModel)!.InboundDirectoryInfos
            .Where(x => x.SubNodes.Any())
            .SelectManyRecursive(x => x.SubNodes).ToList();

        allDirectoryInfos.AddRange(((sender as TreeView)!.DataContext as MainWindowViewModel)!.InboundDirectoryInfos.Where(x => !x.SubNodes.Any()));

        var clickedDirectory = allDirectoryInfos.FirstOrDefault(x => x.FileSystemDirectoryInfo.UniqueId == (long?)clickedDirectoryId);

        if (clickedDirectory == null)
        {
             Log.Warning("Unable to find ClickedDirectory for [{UniqueId}]", clickedDirectoryId);
             return;
        }
        var dc = ((sender as TreeView)!.DataContext as MainWindowViewModel)!;
        var releasesDiscoverer = dc.ReleasesDiscoverer;
        dc.IsLoading = true;
        dc.SelectedRelease = null;        
        dc.SelectedFileSystemDirectoryInfo = clickedDirectory.FileSystemDirectoryInfo;
        using (Operation.Time("Discovering releases for [{File}]", clickedDirectory.FileSystemDirectoryInfo))
        {
            var pagedResult = Task.Run(() => releasesDiscoverer.ReleasesGridsForDirectoryAsync(clickedDirectory.FileSystemDirectoryInfo, new PagedRequest())).Result;
            dc.ReleaseInfos.Clear();
            dc.ReleaseInfos.AddRange(pagedResult.Data);
            dc.IsLoading = false;
        }
    }

    private void DataGrid_OnTapped(object? sender, TappedEventArgs e)
    {
        var textBlock = e.Source as TextBlock;
        if (textBlock == null)
        {
            return;
        }

        var clickedReleaseGrid = textBlock.DataContext as ReleaseGrid;
        if (clickedReleaseGrid == null)
        {
            Log.Warning("Unable to find TextBlock DataContext for sender [{Sender}]", sender);
            return;
        }

        var dc = ((sender as DataGrid)?.DataContext as MainWindowViewModel);
        if (dc == null)
        {
            Log.Warning("Unable to find DataGrid DataContext for sender [{Sender}]", sender);
            return;
        }

        var releasesDiscoverer = dc.ReleasesDiscoverer;
        dc.IsLoading = true;
        using (Operation.Time("Getting Release detail for [{File}]", clickedReleaseGrid.UniqueId))
        {
            var detailResult = Task.Run(() => releasesDiscoverer.ReleaseByUniqueIdAsync(dc.SelectedFileSystemDirectoryInfo, clickedReleaseGrid.UniqueId)).Result;
            dc.SelectedRelease = new ReleaseDetail
            {
                Artist = detailResult.Artist() ?? string.Empty,
                Data = detailResult.Tags?.OrderBy(x => x.SortOrder)
                           .Select(x => new KeyPair
                           {
                               Key = x.Identifier.ToString(),
                               Value = x.Value,
                               StyleClass = x.StyleClass.ToString()
                           }) ?? Array.Empty<KeyPair>(),
                Title = detailResult.ReleaseTitle() ?? string.Empty,
                Tracks = detailResult?.Tracks?.OrderBy(x => x.SortOrder).Select(x => new TrackDetail
                {
                    TrackNumber = x.TrackNumber(),
                    Title = x.Title() ?? string.Empty,
                    Data = x.Tags?.OrderBy(x => x.SortOrder).Select(t => new KeyPair
                    {
                        Key = t.Identifier.ToString(),
                        Value = t.Value,
                        StyleClass = t.StyleClass.ToString()
                    }) ?? Array.Empty<KeyPair>()
                }) ?? Array.Empty<TrackDetail>(),
                Status = detailResult?.Status ?? ReleaseStatus.NeedsAttention,
                Year = detailResult?.ReleaseYear() ?? 0
            };
            dc.IsLoading = false;
        }
    }
}