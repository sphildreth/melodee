namespace Melodee.Common.Enums;

public enum ReleaseResultFilter
{
    NotSet = 0,
    All,
    Duplicates,
    Incomplete,
    LessThanConfiguredTracks,
    New,
    NeedsAttention,
    ReadyToMove,
    Selected,
    LessThanConfiguredDuration
}
