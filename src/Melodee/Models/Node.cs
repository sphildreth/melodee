using System.Collections.ObjectModel;
using Melodee.Common.Models;

namespace Melodee.Models;

public class Node
{
    public DirectoryInfo DirectoryInfo { get; }

    public ObservableCollection<Node> SubNodes { get; set; } = [];
    
    public string Title { get; }
  
    public Node(DirectoryInfo directoryInfo, string title)
    {
        DirectoryInfo = directoryInfo;
        Title = title;
    }

    public Node(DirectoryInfo directoryInfo, string title, ObservableCollection<Node> subNodes)
    {
        DirectoryInfo = directoryInfo;
        Title = title;
        SubNodes = subNodes;
    }
}