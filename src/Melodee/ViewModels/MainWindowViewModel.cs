using System.Collections.ObjectModel;
using DynamicData;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Models;


namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    
    public ObservableCollection<Node> Nodes{ get; }
    
    public ObservableCollection<DirectoryInfo> DirectoryInfos { get; }

    public ObservableCollection<FileInfo> FileInfos { get; }
    
    public MainWindowViewModel()
    {
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