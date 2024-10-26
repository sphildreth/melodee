using Melodee.Common.OrderBy;
using System.Linq.Dynamic.Core;

namespace Melodee.Common.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable StringFilter(this IQueryable source, string term) {
        if (string.IsNullOrEmpty(term))
        {
            return source;
        }
        var elementType = source.ElementType;

        // Retrieve all string properties from this specific type
        var stringProperties = 
            elementType.GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .ToArray();
        if (!stringProperties.Any()) { return source; }

        // Build the string expression
        string filterExpr = string.Join(" || ", stringProperties.Select(prp => $"{prp.Name}.Contains(@0)"));

        return source.Where(filterExpr, term);
    }
    
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.OrderBy(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.OrderByDescending(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.ThenBy(source, orderBy.Expression);
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, IOrderBy orderBy)
    {
        return Queryable.ThenByDescending(source, orderBy.Expression);
    }
}
