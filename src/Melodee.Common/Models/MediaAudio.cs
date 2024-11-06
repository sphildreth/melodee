using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models;

/// <summary>
///     This is a representation of a single MediaAudioIdentifier (like 'Duration') and its value.
/// </summary>
/// <typeparam name="T">Data type of MediaAudioIdentifier.</typeparam>
[Serializable]
public sealed record MediaAudio<T>
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required MediaAudioIdentifier Identifier { get; init; }

    public required T Value { get; init; }

    public int SortOrder { get; init; }
}
