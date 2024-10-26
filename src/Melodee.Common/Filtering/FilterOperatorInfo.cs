namespace Melodee.Common.Filtering;

/// <summary>
/// Filter operator to dynamically filter IQueryable results.
/// </summary>
/// <param name="PropertyName">Name of object type being filtered.</param>
/// <param name="Operator">Operation of Filter.</param>
/// <param name="Value">Value to filter on. If string then wrap in quotes as necessary.</param>
/// <param name="JoinOperator">The Join condition when more than one filter, e.g. '||' or '&&'</param>
public record FilterOperatorInfo(string PropertyName, FilterOperator Operator, string Value, string? JoinOperator = "&&")
{
    public static string FilterOperatorToConditionString(FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.NotEquals => "!=",
            FilterOperator.LessThan => "<",
            FilterOperator.LessThanOrEquals => "<=",
            FilterOperator.GreaterThan => ">",
            FilterOperator.GreaterThanOrEquals => ">=",
            FilterOperator.Contains => "LIKE",
            FilterOperator.StartsWith => "LIKE",
            FilterOperator.EndsWith => "LIKE",
            FilterOperator.DoesNotContain => "NOT LIKE",
            FilterOperator.IsNull => "ISNULL",
            FilterOperator.IsNotNull => "ISNULL",
            _ => "=="
        };
    }
}
