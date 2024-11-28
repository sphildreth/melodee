namespace Melodee.Common.Models;

public sealed record PagedResult<T> : OperationResult<IEnumerable<T>> where T : notnull
{
    public PagedResult(IEnumerable<string>? messages = null)
        : base(messages)
    {
    }

    public int CurrentPage { get; init; }

    public long TotalCount { get; init; }

    public int TotalPages { get; init; }
}
