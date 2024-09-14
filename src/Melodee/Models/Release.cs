using Melodee.Common.Models.Cards;

namespace Melodee.Models;

public sealed record Release : ReleaseCard
{
    public bool Selected { get; set; }
    public int Index {get; set;}

    public string ImageBase64String(byte[] defaultImage)
    {
        return $"data:image/png;base64,{ Convert.ToBase64String(ImageBytes ?? defaultImage) }";
    }
}
