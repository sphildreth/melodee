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
    private readonly Configuration _configuration = configuration;

    [ObservableProperty]
    private Visibility _progressRingVisibility = Visibility.Collapsed;

    [ObservableProperty] 
    private IEnumerable<ReleaseCard> _releases = [];
    
    [ObservableProperty] 
    private string? _searchText;

    public async Task LoadStagingReleases(int page = 1, short take = 10)
    {
        ProgressRingVisibility = Visibility.Visible;
        var releasesForStagingFolder = await _releasesDiscoverer.ReleasesGridsForDirectoryAsync(
            _configuration.StagingDirectoryInfo, new PagedRequest
            {
                Page = page,
                Take = take
            });
        Releases = releasesForStagingFolder.Data.AsQueryable().ProjectToType<ReleaseCard>().ToArray();
        ProgressRingVisibility = Visibility.Collapsed;
    }
}
