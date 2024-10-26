using System.Linq.Expressions;

namespace Melodee.Common.OrderBy;

public class OrderBy<TSearchResultItem, T>(Expression<Func<TSearchResultItem, T>> expression) : IOrderBy
{
    public dynamic Expression => expression;
}
