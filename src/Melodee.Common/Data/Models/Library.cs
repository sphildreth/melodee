using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class Library : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string Path { get; set; }
    
    public Instant? LastScanAt { get; set; }
}
