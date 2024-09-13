using Melodee.Common.Models.Cards;

namespace Melodee.Models;

public sealed record Release : ReleaseCard
{
    public bool Selected { get; set; }
    public int Index {get; set;}
}
