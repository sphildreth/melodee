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
    private readonly Configuration _configuration;
    private readonly IDirectoryDiscoverer _directoryDiscoverer;
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    
    public ObservableCollection<Node> Nodes
    {
        get
        {
            var directories =
                   _directoryDiscoverer.DirectoryInfosForDirectory(
                    new System.IO.DirectoryInfo(_configuration.InboundDirectory), new PagedRequest());

            var nodes = new List<Node>();

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
            }
            return new ObservableCollection<Node>(nodes);
        }
    }
    
    public ObservableCollection<DirectoryInfo> DirectoryInfos { get; }

    public ObservableCollection<FileInfo> FileInfos { get; }
    
    public MainWindowViewModel(Configuration configuration, IDirectoryDiscoverer directoryDiscoverer)
    {
        _configuration = configuration;
        _directoryDiscoverer = directoryDiscoverer;

        // Nodes = new ObservableCollection<Node>
        // {                
        //     new Node("Incoming", new ObservableCollection<Node>
        //     {
        //         new Node("Mammals", new ObservableCollection<Node>
        //         {
        //             new Node("Lion"), new Node("Cat"), new Node("Zebra")
        //         })
        //     }),
        //     new Node("Library", new ObservableCollection<Node>
        //     {
        //         new Node("Mammals", new ObservableCollection<Node>
        //         {
        //             new Node("Lion"), new Node("Cat"), new Node("Zebra")
        //         })
        //     })            
        // };

        DirectoryInfos = new ObservableCollection<DirectoryInfo>
        {
            new DirectoryInfo { ShortName = "DaDirectoryName", Path = "DaPath", MusicFilesFound = 0, TotalItemsFound = 0}
        };

        FileInfos = new ObservableCollection<FileInfo>
        {
            new FileInfo
            {
                DirectoryInfo = new DirectoryInfo { Path = "DaPath", ShortName = "DaDirectoryName", MusicFilesFound = 0, TotalItemsFound = 0},
                Name = "Da Name",
                Path = "Da Path File",
                Status = FileInfoStatus.Ok
            }
        };
    }    
}