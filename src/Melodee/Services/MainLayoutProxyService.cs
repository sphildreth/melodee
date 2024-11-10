namespace Melodee.Services;

/// <summary>
/// This is used by child pages to notify the MainLayout that it should show text or the activity spinner.
/// </summary>
public sealed class MainLayoutProxyService
{
    public bool ShowSpinner { get; set; }
    
    public string Header { get; set; } = "Loading...";

    public event EventHandler? HeaderChanged;
    public event EventHandler? SpinnerVisibleChanged;

    public void SetHeader(string header)
    {
        Header = header;
        HeaderChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleSpinnerVisible(bool? forceState = null)
    {
        ShowSpinner = forceState ?? !ShowSpinner;
        SpinnerVisibleChanged?.Invoke(this, EventArgs.Empty);
    }

    public void NotifyHeaderChanged()
        => HeaderChanged?.Invoke(this, EventArgs.Empty);
    
    public void NotifySpinnerVisibleChangedChanged()
        => SpinnerVisibleChanged?.Invoke(this, EventArgs.Empty);    
}
