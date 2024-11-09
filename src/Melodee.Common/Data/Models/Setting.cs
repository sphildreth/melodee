using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(Key), IsUnique = true)]
[Index(nameof(Category))]
public class Setting : DataModelBase
{
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string Key { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Comment { get; set; }

    public int? Category { get; set; }

    [NotMapped] public SettingCategory CategoryValue => Category == null ? SettingCategory.General : SafeParser.ToEnum<SettingCategory>(Category);

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public required string Value { get; set; }
}
