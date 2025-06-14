using Melodee.Blazor.Controllers.Melodee.Models;
using MelodeeCommonModels = Melodee.Common.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class StatisticExtensions
{
    public static Statistic ToStatisticModel(this MelodeeCommonModels.Statistic statistic)
    {
        return new Statistic(statistic.Type,
            statistic.Title,
            statistic.Data.ToString() ?? string.Empty,
            statistic.Message,
            statistic.SortOrder
        );
    }
}
