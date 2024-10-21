using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(Client), nameof(UserAgent))]
public class Player : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Name { get; set; }
    
    public string? UserAgent { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string Client { get; set; }
    
    public string? IpAddress { get; set; }
    
    [Required]
    public required Instant LastSeenAt { get; set; }
    
    public int? MaxBitRate { get; set; }
    
    public bool ScrobbleEnabled { get; set; }
}
