using System.ComponentModel.DataAnnotations;
using Melodee.Common.Models.OpenSubsonic;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(ArtistId), IsUnique = true)]
public class UserArtist : DataModelBase
{
    [Required]
    public required int UserId { get; set; }
    
    [Required]
    public required int ArtistId { get; set; }
  
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
