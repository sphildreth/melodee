using System.Text;
using System.Linq.Dynamic.Core;
using Melodee.Common.Enums;
using Melodee.Common.Filtering;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record PagedRequest
{
    private const int DefaultPageSize = 100;

    public const string OrderAscDirection = "ASC";
    public const string OrderDescDirection = "DESC";
    public const string DefaultSortField = "Id";
    private int? _skipValue;

    public short? PageSize { get; set; } = DefaultPageSize;
    public string? Search { get; init; }

    public AlbumResultFilter? AlbumResultFilter { get; init; }

    /// <summary>
    /// Filter by definitions. 
    /// </summary>
    public FilterOperatorInfo[]? FilterBy { get; init; }

    public long[] SelectedAlbumIds { get; init; } = Array.Empty<long>();

    public short PageSizeValue
    {
        get
        {
            if (PageSize is -1)
            {
                // Suppose to mean return all, this limits to something sane other than Int.MaxLimit
                return 500;
            }

            var result = PageSize ?? DefaultPageSize;
            return result < 1 ? (short)DefaultPageSize : result;
        }
    }

    public int? Page { get; init; } = 1;

    public int PageValue => Page ?? 1;

    /// <summary>
    /// When this is true then only return the count of records for the request, do not return any actual records.
    /// </summary>
    public bool IsTotalCountOnlyRequest { get; set; }
    
    /// <summary>
    /// Sort By definitions. PropertyName and Direction, e.g. 'Id', 'Desc'
    /// </summary>
    public Dictionary<string, string>? OrderBy { get; set; }

    public int SkipValue
    {
        get
        {
            if (!_skipValue.HasValue)
            {
                if (Page.HasValue)
                {
                    _skipValue = Page.Value * PageSizeValue - PageSizeValue;
                }
                else
                {
                    return 0;
                }
            }

            return _skipValue.Value;
        }
        set => _skipValue = value;
    }

    public string OrderByValue(string? defaultSortBy = null, string? defaultOrderBy = null)
    {
        var result = new StringBuilder();
        if (OrderBy == null)
        {
            OrderBy = new Dictionary<string, string>()
            {
                { defaultSortBy ?? DefaultSortField, defaultOrderBy ?? OrderAscDirection }
            };
        }
        if (OrderBy != null && OrderBy.Count != 0)
        {
            foreach (var kp in OrderBy)
            {
                if (result.Length > 0)
                {
                    result.Append(",");
                }

                result.AppendFormat("\"{0}\" {1}", kp.Key, kp.Value);
            }
        }
        return result.ToString();
    }
    
    public string FilterByValue()
    {
        if (FilterBy == null || FilterBy.Length == 0)
        {
            return "1 = 1";
        }
        var result = new StringBuilder();
        foreach (var kp in FilterBy)
        {
            if (result.Length > 0)
            {
                result.Append($" {kp.JoinOperator } ");
            }

            result.AppendFormat("{0} {1} {2}", kp.PropertyName, kp.Operator, kp.Value);
        }
        return result.ToString();
    }    

    public int TotalPages(int totalRecordsCount) => totalRecordsCount < 1 ? 0 : (totalRecordsCount + PageSizeValue - 1) / PageSizeValue;

}
