using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DynamicData;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Models;
using Melodee.Plugins.Discovery.Directory;
using ReactiveUI;


namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Configuration Configuration { get; }
    
    public IDirectoryDiscoverer DirectoryDiscoverer { get; }
    
    public ObservableCollection<Node> InboundDirectoryInfos { get; }
    
    public ObservableCollection<FileInfo> FileInfos { get; }
    
    public MainWindowViewModel(Configuration configuration, IDirectoryDiscoverer directoryDiscoverer)
    {
        Configuration = configuration;
        DirectoryDiscoverer = directoryDiscoverer;

        var directories =
            DirectoryDiscoverer.DirectoryInfosForDirectory(
                new System.IO.DirectoryInfo(Configuration.InboundDirectory), new PagedRequest());

        var result = new List<Node>();
        var dirData = directories.Data.Rows.ToArray();
        foreach (var directory in dirData)
        {
            var node = new Node(directory.ShortName);
            var hasChildren = dirData.Any(x => x.ParentId == directory.Id);
            if (hasChildren)
            {
                node.SubNodes = new ObservableCollection<Node>(dirData.Where(x => x.ParentId == directory.Id)
                    .Select(x => new Node(x.ShortName)));
            }
            result.Add(node);
        }
        InboundDirectoryInfos = new ObservableCollection<Node>(result);        
        
        FileInfos = new ObservableCollection<FileInfo>
        {
            new FileInfo
            {
                DirectoryInfo = new DirectoryInfo { Path = "DaPath", ShortName = "DaDirectoryName", MusicFilesFound = 0, TotalItemsFound = 0},
                Name = "Da Name",
                Extension = "mp3",
                Path = "Da Path File",
                Status = FileInfoStatus.Ok
            }
        };
    }    
}