using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Song record.
/// <remarks>See https://opensubsonic.netlify.app/docs/responses/child/</remarks>
/// </summary>
[Index(nameof(Title))]
[Index(nameof(AlbumDiscId), nameof(TrackNumber), IsUnique = true)]
public class Song : MetaDataModelBase
{
    
    // full_text varchar(255) default '', album_artist_id varchar(255) default '', date varchar(255) default '' not null, original_year int default 0 not null, original_date varchar(255) default '' not null, release_year int default 0 not null, release_date varchar(255) default '' not null, order_album_name varchar not null default '', order_album_artist_name varchar not null default '', order_artist_name varchar not null default '', sort_album_name varchar not null default '', sort_artist_name varchar not null default '', sort_album_artist_name varchar not null default '', sort_title varchar not null default '', disc_subtitle varchar not null default '', catalog_num varchar not null default '', comment varchar not null default '', order_title varchar not null default '', mbz_recording_id varchar not null default '', mbz_album_id varchar not null default '', mbz_artist_id varchar not null default '', mbz_album_artist_id varchar not null default '', mbz_album_type varchar not null default '', mbz_album_comment varchar not null default '', mbz_release_track_id varchar not null default '', bpm integer not null default 0, channels integer not null default 0, rg_album_gain real not null default 0, rg_album_peak real not null default 0, rg_track_gain real not null default 0, rg_track_peak real not null default 0, lyrics jsonb not null default '[]', sample_rate integer not null default 0, library_id integer not null default 1 
    // references library(id) on delete cascade);
        
    public int AlbumDiscId { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Title { get; set; }    
    
    public required int TrackNumber { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string FilePath { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    [Required]
    public required string FileName { get; set; }
    
    [Required]
    public required int FileSize { get; set; }
    
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    [Required]
    public required string FileHash { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? PartTitles { get; set; }
    
    [Required]
    public required int Duration { get; set; }
    
    [Required]
    public required int SamplingRate { get; set; }
    
    [Required]
    public required int BitRate { get; set; }
    
    [Required]
    public required int BitDepth { get; set; }
    
    [Required]
    public required int BPM { get; set; }
    
    public int? ChannelCount { get; set; }
}
