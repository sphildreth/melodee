using System.Text;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic;

public sealed record PlayQueue : IOpenSubsonicToXml
{
    /// <summary>
    /// This is the ID of the item in the Playlist (or PlayQueue) not any database PkId.
    /// </summary>
    public required int Current { get; init; }
    
    public required double Position { get; init; }

    public required string ChangedBy { get; init; }

    public required string Changed { get; init; }

    public required string Username { get; init; }

    public Child[]? Entry { get; init; }
    
    public string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder($"<playQueue current=\"{ Current }\" position=\"{ Position }\" username=\"{ Username}\" changed=\"{Changed}\" changedBy=\"{ ChangedBy}\">");
        if (Entry != null)
        {
            foreach (var child in Entry)
            {
                result.Append(child.ToXml("entry"));
            }
        }
        result.Append("</playQueue>");
        return result.ToString();
    }    
}
