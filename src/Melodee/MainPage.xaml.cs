using System.Reflection;
using Melodee.Common.Models;
using Melodee.ViewModels;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Melodee;

public sealed partial class MainPage : Page
{
    private byte[] _defaultReleaseCover = [];

    public MainPage()
    {
        InitializeComponent();

        var mainModel = (Application.Current as App)?.Host.Services.GetRequiredService<MainModel>();
        DataContext = mainModel;
        Loaded += async (s, e) => await mainModel!.LoadStagingReleases();
    }


    private void LoadReleaseImage(object sender, RoutedEventArgs e)
    {
        // ensure default release cover bytes are loaded
        if (_defaultReleaseCover.Length == 0)
        {
            var appDir = new DirectoryInfo(Assembly.GetEntryAssembly()!.Location);
            _defaultReleaseCover = File.ReadAllBytes(Path.Combine(appDir.Parent.FullName, "Assets", "Images", "release.jpg"));
        }
        if (sender is Image s)
        {
            var imageBytes = s.Tag as byte[] ?? _defaultReleaseCover;
            var bitmap = new BitmapImage();
            using (var ms = new MemoryStream(imageBytes))
            {
                bitmap.SetSource(ms.AsRandomAccessStream());
            }
            s.Source = bitmap;
        }
    }

    private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e) => (DataContext as MainModel)!.LoadReleaseDetail((ReleaseCard)sender);

    private void ReleaseDetail_Artist_Image_OnLoaded(object sender, RoutedEventArgs e)
    {
        //throw new NotImplementedException();
    }
}
