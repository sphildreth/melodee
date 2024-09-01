using Melodee.Common.Models;
using Melodee.ViewModels;
using Microsoft.Extensions.Hosting;

namespace Melodee;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        var mainModel = (Application.Current as App)?.Host.Services.GetRequiredService<MainModel>();
        DataContext = mainModel;
        Loaded += async(s,e) => await mainModel!.LoadStagingReleases(1, 100);
    }
    

    private void LoadReleaseImage(object sender, RoutedEventArgs e)
    {
        var stagingDirectoryPath = (DataContext as MainModel).Configuration.StagingDirectory;

        // var uri = new System.Uri("ms-appdata:///local/images/logo.png");
        // var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
        //
        // Image img = new Image();
        // img.Source = file;
        
        // Image img = sender as Image;
        // if (img != null)
        // {
        //     BitmapImage bitmapImage = new BitmapImage();
        //     img.Width = bitmapImage.DecodePixelWidth = 280;
        //     bitmapImage.UriSource = new Uri("ms-appx:///Assets/Logo.png");
        //     img.Source = bitmapImage;
        // }
        
        throw new NotImplementedException();
    }
}
