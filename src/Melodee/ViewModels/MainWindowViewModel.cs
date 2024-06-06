using System.Collections.ObjectModel;
using DynamicData;
using Melodee.Models;


namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    
    public ObservableCollection<Node> Nodes{ get; }
    
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
    }    
}