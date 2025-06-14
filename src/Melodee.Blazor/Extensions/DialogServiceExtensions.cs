using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;

namespace Melodee.Blazor.Extensions;

/// <summary>
/// Extensions for the Radzen DialogService
/// </summary>
public static class DialogServiceExtensions
{
    /// <summary>
    /// Creates a confirmation dialog with HTML content from a string
    /// </summary>
    /// <param name="dialogService">The dialog service</param>
    /// <param name="htmlContent">HTML content as string</param>
    /// <param name="title">Title of dialog</param>
    /// <param name="options">Dialog options</param>
    /// <returns>True if confirmed, otherwise false</returns>
    public static async Task<bool?> ConfirmHtml(this DialogService dialogService, 
        string htmlContent, 
        string title = "Confirm", 
        ConfirmOptions? options = null)
    {
        return await dialogService.Confirm(builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddMarkupContent(1, htmlContent);
            builder.CloseElement();  
        }, title, options ?? new ConfirmOptions());
    }
}

