using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(NameNormalized))]
[Index(nameof(SortName))]
public abstract class NamedMetaDataModelBase : MetaDataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public virtual required string Name { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string NameNormalized { get; set; }    
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SortName { get; set; }
}
