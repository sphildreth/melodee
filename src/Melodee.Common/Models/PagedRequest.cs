using System.Text;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record PagedRequest
{
    private const int DefaultPageSize = 100;

    public const string OrderAscDirection = "ASC";
    public const string OrderDescDirection = "DESC";
    private int? _skipValue;

    public short? PageSize { get; set; } = DefaultPageSize;
    public string? Search { get; init; }

    public AlbumResultFilter? Filter { get; init; }

    public long[] SelectedAlbumIds { get; init; } = Array.Empty<long>();

    public short TakeValue
    {
        get
        {
            if (PageSize is -1)
            {
                // Suppose to mean return all, this limits to something sane other than Int.MaxLimit
                return 500;
            }

            return PageSize ?? DefaultPageSize;
        }
    }

    public string? Order { get; init; }
    public int? Page { get; init; } = 1;

    public int PageValue => Page ?? 1;

    /// <summary>
    /// When this is true then only return the count of records for the request, do not return any actual records.
    /// </summary>
    public bool IsTotalCountOnlyRequest { get; set; }
    
    public string? Sort { get; set; }

    public int SkipValue
    {
        get
        {
            if (!_skipValue.HasValue)
            {
                if (Page.HasValue)
                {
                    _skipValue = Page.Value * TakeValue - TakeValue;
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

    /// <summary>
    ///     Sort first with the given (if any) parameter then apply default sorting. Example is "rating" supplied then sort by
    ///     sortName
    /// </summary>
    public string OrderValue(Dictionary<string, string>? orderBy = null, string? defaultSortBy = null, string? defaultOrderBy = null)
    {
        var result = new StringBuilder();
        if (!string.IsNullOrEmpty(Sort))
        {
            result.AppendFormat("{0} {1}", Sort ?? defaultSortBy, Order ?? defaultOrderBy ?? OrderAscDirection);
        }

        if (orderBy != null && orderBy.Count != 0)
        {
            foreach (var kp in orderBy)
            {
                if (result.Length > 0)
                {
                    result.Append(",");
                }

                result.AppendFormat("{0} {1}", kp.Key, kp.Value);
            }
        }

        return result.ToString();
    }

    public int TotalPages(int totalRecordsCount) => (totalRecordsCount + TakeValue - 1) / TakeValue;
}
