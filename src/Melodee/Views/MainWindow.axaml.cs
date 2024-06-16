using System;
using Avalonia.Controls;
using Avalonia.Input;
using Melodee.ViewModels;
using ReactiveUI;

namespace Melodee.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationTree_OnTapped(object? sender, TappedEventArgs e)
    {
        var clickedDirectory = (e.Source as TextBlock)!.Text;
        
        //var fileDiscoverer = ((sender as TreeView).DataContext as MainWindowViewModel).FileDiscoverer
        
        Console.WriteLine(e.Timestamp);
        //   throw new System.NotImplementedException();
    }
}