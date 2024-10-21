using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models;

public class RadioStation : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string StreamUrl { get; set; }
    
    public string? HomePageUrl { get; set; }
}
