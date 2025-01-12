using Melodee.Common.Data.Models;
using Melodee.Common.Enums;

namespace Melodee.Blazor.Extensions.Data;

public static class LibraryExtensions
{
    public static (string, string) LibraryTypeIconAndTitle(this Library library)
    {
        return library.TypeValue switch
        {
            LibraryType.Inbound or LibraryType.Staging => ("stock_media", "Media queue type library"),
            LibraryType.UserImages => ("photo_library", "Images type library"),
            _ => ("inventory_2","Storage type library")
        };
    }
    
    
}
