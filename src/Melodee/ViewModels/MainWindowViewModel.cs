using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DynamicData;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Grids;
using Melodee.Models;
using Melodee.Plugins.Discovery.Directories;
using Melodee.Plugins.Discovery.Releases;
using ReactiveUI;


namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Configuration Configuration { get; }
    
    public IDirectoriesDiscoverer DirectoriesDiscoverer { get; }
    
    public IReleasesDiscoverer ReleasesDiscoverer { get; }
    
    public ObservableCollection<Node> InboundDirectoryInfos { get; }

    public ObservableCollection<ReleaseGrid> ReleaseInfos { get; set; } = [];
    
    public MainWindowViewModel(Configuration configuration, IDirectoriesDiscoverer directoriesDiscoverer, IReleasesDiscoverer releasesDiscoverer)
    {
        Configuration = configuration;
        DirectoriesDiscoverer = directoriesDiscoverer;
        ReleasesDiscoverer = releasesDiscoverer;

        var directories =
            DirectoriesDiscoverer.DirectoryInfosForDirectory(
                new System.IO.DirectoryInfo(Configuration.InboundDirectory), new PagedRequest());

        var result = new List<Node>();
        var dirData = directories.Data.ToArray();
        foreach (var directory in dirData)
        {
            var node = new Node(directory, directory.ShortName);
            var hasChildren = dirData.Any(x => x.ParentId == directory.UniqueId);
            if (hasChildren)
            {
                node.SubNodes = new ObservableCollection<Node>(dirData.Where(x => x.ParentId == directory.UniqueId)
                    .Select(x => new Node(x, x.ShortName) {  }));
            }
            result.Add(node);
        }
        InboundDirectoryInfos = new ObservableCollection<Node>(result);      

    }    
}