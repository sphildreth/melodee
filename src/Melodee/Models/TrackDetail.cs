using System.Collections.Generic;

namespace Melodee.Models;

public sealed record TrackDetail
{
    public required int TrackNumber { get; init; }
    
    public required string Title { get; init; }
    
    public required IEnumerable<KeyPair> Data { get; init; }
}