using System.Text;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic;

public sealed record PlayQueue : IOpenSubsonicToXml
{
    /// <summary>
    ///     This is the Id of the Child that is the currently playing song.
    /// </summary>
    public required string Current { get; init; }

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
            foreach (var allowedUser in Entry)
            {
                result.Append(allowedUser.ToXml());
            }
        }
        result.Append("</playQueue>");
        return result.ToString();
    }    
}
