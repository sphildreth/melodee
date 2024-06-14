using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Melodee.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationTree_OnTapped(object? sender, TappedEventArgs e)
    {
        Console.WriteLine(e.Timestamp);
        //   throw new System.NotImplementedException();
    }
}