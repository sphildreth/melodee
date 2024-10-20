namespace Melodee.Common.Enums;

public enum AlbumResultFilter
{
    NotSet = 0,
    All,
    Duplicates,
    Incomplete,
    LessThanConfiguredSongs,
    New,
    NeedsAttention,
    ReadyToMove,
    Selected,
    LessThanConfiguredDuration
}
