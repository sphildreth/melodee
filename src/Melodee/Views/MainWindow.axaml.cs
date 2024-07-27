using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Models;
using Melodee.ViewModels;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Views;

public partial class MainWindow : Window
{
    private readonly Configuration _configuration;

    public MainWindow(Configuration configuration)
    {
        _configuration = configuration;
        InitializeComponent();
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
        using (Operation.At(LogEventLevel.Debug).Time("Getting Release detail for [{File}]", clickedReleaseGrid.UniqueId))
        {
            var detailResult = Task.Run(() => releasesDiscoverer.ReleaseByUniqueIdAsync(_configuration.StagingDirectoryInfo, clickedReleaseGrid.UniqueId)).Result;
            dc.SelectedRelease = new ReleaseDetail
            {
                UniqueId = clickedReleaseGrid.UniqueId,
                Artist = detailResult.Artist() ?? string.Empty,
                Data = detailResult.Tags?.OrderBy(x => x.SortOrder)
                           .Select(x => new KeyPair
                           {
                               Key = x.Identifier.ToString(),
                               Value = x.Value,
                               StyleClass = x.StyleClass.ToString()
                           }) ?? Array.Empty<KeyPair>(),
                Title = detailResult.ReleaseTitle() ?? string.Empty,
                Tracks = detailResult.Tracks?.OrderBy(x => x.SortOrder).Select(x => new TrackDetail
                {
                    TrackNumber = x.TrackNumber(),
                    Title = x.Title() ?? string.Empty,
                    Data = x.Tags?.OrderBy(t => t.SortOrder).Select(t => new KeyPair
                    {
                        Key = t.Identifier.ToString(),
                        Value = t.Value,
                        StyleClass = t.StyleClass.ToString()
                    }) ?? Array.Empty<KeyPair>()
                }) ?? Array.Empty<TrackDetail>(),
                Status = detailResult.Status,
                Year = detailResult.ReleaseYear() ?? 0
            };
            dc.IsLoading = false;
        }
    }
}