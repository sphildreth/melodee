using Melodee.Common.Enums;

namespace Melodee.Common.Models;

public sealed record Statistic(
    StatisticType Type,
    string Title,
    object Data,
    string? DisplayColor,
    string? Message = null,
    short? SortOrder = null,
    string? Icon = null,
    bool? IncludeInApiResult = null);
