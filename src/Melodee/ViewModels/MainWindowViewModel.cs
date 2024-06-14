using System.Collections.ObjectModel;
using DynamicData;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Models;
using Melodee.Plugins.Discovery.Directory;


namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDirectoryDiscoverer _directoryDiscoverer;
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    
    public ObservableCollection<Node> Nodes{ get; }
    
    public ObservableCollection<DirectoryInfo> DirectoryInfos { get; }

    public ObservableCollection<FileInfo> FileInfos { get; }
    
    public MainWindowViewModel(IDirectoryDiscoverer directoryDiscoverer)
    {
        _directoryDiscoverer = directoryDiscoverer;

        Nodes = new ObservableCollection<Node>
        {                
            new Node("Incoming", new ObservableCollection<Node>
            {
                new Node("Mammals", new ObservableCollection<Node>
                {
                    new Node("Lion"), new Node("Cat"), new Node("Zebra")
                })
            }),
            new Node("Library", new ObservableCollection<Node>
            {
                new Node("Mammals", new ObservableCollection<Node>
                {
                    new Node("Lion"), new Node("Cat"), new Node("Zebra")
                })
            })            
        };

        DirectoryInfos = new ObservableCollection<DirectoryInfo>
        {
            new DirectoryInfo { Path = "DaPath", MusicFilesFound = 0, TotalItemsFound = 0}
        };

        FileInfos = new ObservableCollection<FileInfo>
        {
            new FileInfo
            {
                DirectoryInfo = new DirectoryInfo { Path = "DaPath", MusicFilesFound = 0, TotalItemsFound = 0},
                Name = "Da Name",
                Path = "Da Path File",
                Status = FileInfoStatus.Ok
            }
        };
    }    
}