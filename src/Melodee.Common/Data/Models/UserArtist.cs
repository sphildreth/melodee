using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(ArtistId), IsUnique = true)]
public class UserArtist : DataModelBase
{
    [RequiredGreaterThanZero]
    public required int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [RequiredGreaterThanZero]
    public required int ArtistId { get; set; }
    
    public Artist Artist { get; set; } = null!;
  
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
