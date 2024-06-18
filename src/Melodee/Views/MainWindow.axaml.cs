using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Melodee.Common.Models;
using Melodee.Common.Models.Grids;
using Melodee.ViewModels;
using ReactiveUI;

namespace Melodee.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationTree_OnTapped(object? sender, TappedEventArgs e)
    {
        var clickedDirectoryId = (e.Source as TextBlock)?.Tag as long?;
        if (!clickedDirectoryId.HasValue)
        {
            return;
        }
        var clickedDirectory =
            ((sender as TreeView)!.DataContext as MainWindowViewModel)!.InboundDirectoryInfos
            .First(x => x.DirectoryInfo.UniqueId == clickedDirectoryId);
        var dc = ((sender as TreeView)!.DataContext as MainWindowViewModel)!;
        var fileDiscoverer = dc.ReleasesDiscoverer;
        var fileDiscovererResult = fileDiscoverer.ReleasesForDirectoryAsync(clickedDirectory.DirectoryInfo, new PagedRequest()).Result;
        dc.ReleaseInfos = new ObservableCollection<ReleaseGrid>(fileDiscovererResult.Data);
    }
}