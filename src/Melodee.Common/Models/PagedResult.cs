namespace Melodee.Common.Models;

[Serializable]
public sealed record PagedResult<T> where T : notnull
{
    public int CurrentPage { get; init; }

    public bool IsSuccess => Message == OperationMessages.OkMessage;

    public string? Message { get; init; }

    public long OperationTime { get; init; }

    public required IEnumerable<T> Rows { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }    
}