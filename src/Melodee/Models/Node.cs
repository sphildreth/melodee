using System.Collections.ObjectModel;
using Melodee.Common.Models;

namespace Melodee.Models;

public class Node
{
    public FileSystemDirectoryInfo FileSystemDirectoryInfo { get; }

    public ObservableCollection<Node> SubNodes { get; set; } = [];
    
    public string Title { get; }
  
    public Node(FileSystemDirectoryInfo fileSystemDirectoryInfo, string title)
    {
        FileSystemDirectoryInfo = fileSystemDirectoryInfo;
        Title = title;
    }

    public Node(FileSystemDirectoryInfo fileSystemDirectoryInfo, string title, ObservableCollection<Node> subNodes)
    {
        FileSystemDirectoryInfo = fileSystemDirectoryInfo;
        Title = title;
        SubNodes = subNodes;
    }
}