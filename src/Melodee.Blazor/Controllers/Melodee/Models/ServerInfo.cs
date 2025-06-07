namespace Melodee.Blazor.Controllers.Melodee.Models;

public record ServerInfo(string Name, string Description, int MajorVersion, int MinorVersion, int PatchVersion)
{
    public string Version => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
}
