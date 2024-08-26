using CommunityToolkit.Mvvm.ComponentModel;

namespace Melodee.ViewModels;

internal partial class ReleaseCard : ObservableObject
{
    [ObservableProperty] 
    private byte[] _imageBytes = [];

    [ObservableProperty] 
    private string _releaseId = string.Empty;
    
    [ObservableProperty]
    private string _artist = string.Empty;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _infoLine = string.Empty;
}
