using System.Collections;
using System.Collections.Generic;
using Avalonia.Input;
using Melodee.Common.Enums;

namespace Melodee.Models;

public sealed record ReleaseDetail
{
    public required string Artist { get; init; }
    
    public required string Title { get; init; }
    
    public required int Year { get; init; }
    
    public required ReleaseStatus Status { get; init; }
    
    public required IEnumerable<KeyPair> Data { get; init; }
    
    public required IEnumerable<TrackDetail> Tracks { get; init; }
    public required long UniqueId { get; init; }

    public override string ToString() => $"UniqueId [{UniqueId}] Artist [{Artist}] Year [{Year}] Title [{Title}]";
}