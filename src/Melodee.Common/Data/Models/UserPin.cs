using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(PinId), nameof(PinType), IsUnique = true)]
public class UserPin : DataModelBase
{
    [RequiredGreaterThanZero] public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public required int PinId { get; set; }

    [RequiredGreaterThanZero] public required int PinType { get; set; }

    [NotMapped] public UserPinType PinTypeValue => SafeParser.ToEnum<UserPinType>(PinType);

    [NotMapped] public string ImageUrl { get; set; } = string.Empty;

    [NotMapped] public string Icon { get; set; } = string.Empty;

    [NotMapped] public string Text { get; set; } = string.Empty;

    [NotMapped] public string LinkUrl { get; set; } = string.Empty;
}
