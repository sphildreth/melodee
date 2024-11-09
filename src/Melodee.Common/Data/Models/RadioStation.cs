using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models;

[Serializable]
public class RadioStation : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public required string StreamUrl { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? HomePageUrl { get; set; }
}
