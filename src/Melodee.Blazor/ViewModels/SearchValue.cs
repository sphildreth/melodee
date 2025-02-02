namespace Melodee.Blazor.ViewModels;

public class SearchValue
{
    public required string Key { get; set; }

    public required string Value { get; set; }

    public int SortOrder { get; set; }
}
