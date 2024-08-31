using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Discovery.Releases;
using Microsoft.Extensions.Options;

namespace Melodee.ViewModels;

internal partial class MainModel(
    IReleasesDiscoverer releasesDiscoverer,
    Configuration configuration)
    : ObservableObject
{
    private readonly IReleasesDiscoverer _releasesDiscoverer = releasesDiscoverer;
    
    public Configuration Configuration { get; } = configuration;

    [ObservableProperty]
    private Visibility _progressRingVisibility = Visibility.Collapsed;

    [ObservableProperty] 
    private IEnumerable<ReleaseCard> _releases = [];
    
    [ObservableProperty] 
    private string? _searchText;

    public async Task LoadStagingReleases(int page = 1, short take = 100)
    {
        ProgressRingVisibility = Visibility.Visible;
        var releasesForStagingFolder = await _releasesDiscoverer.ReleasesGridsForDirectoryAsync(
            Configuration.StagingDirectoryInfo, new PagedRequest
            {
                Page = page,
                Take = take
            });
        Releases = releasesForStagingFolder.Data.AsQueryable().ProjectToType<ReleaseCard>().ToArray();
        ProgressRingVisibility = Visibility.Collapsed;
    }
}
