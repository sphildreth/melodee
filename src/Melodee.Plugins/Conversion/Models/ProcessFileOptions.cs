namespace Melodee.Plugins.Conversion.Models;

[Serializable]
public sealed record ProcessFileOptions
{
    public bool DoDeleteOriginal { get; init; } = true;
    
}