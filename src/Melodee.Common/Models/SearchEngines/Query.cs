using System.ComponentModel.DataAnnotations;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

public record Query
{
    public Guid? ApiKey { get; init; }

    [Required] public required string Name { get; init; }

    private string NameInTagFormat
    {
        get
        {
            if (Name.Contains(','))
            {
                return "".AddTag(Name.Split(','), doReorder: false)?.ToNormalizedString() ?? NameNormalized;
            }

            if (Name.Contains(' '))
            {
                return "".AddTag(Name.Split(' '), doReorder: false)?.ToNormalizedString() ?? NameNormalized;
            }

            return NameNormalized;
        }
    }

    public string NameNormalizedReversed => string.Join(string.Empty, NameInTagFormat.ToTags()!.Reverse()).ToNormalizedString() ?? Name;

    public string NameNormalized => Name.ToNormalizedString() ?? Name;

    public string QueryNameNormalizedValue => NameNormalized.Nullify() == null ? string.Empty : $"%{NameNormalized}%";

    public Guid? MusicBrainzIdValue => SafeParser.ToGuid(MusicBrainzId);

    public string? MusicBrainzId { get; init; }
}
