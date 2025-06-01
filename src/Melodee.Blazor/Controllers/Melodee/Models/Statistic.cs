using Melodee.Common.Enums;

namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Statistic(
    StatisticType Type,
    string Title,
    string Data,
    string? Description = null,
    short? SortOrder = null);
