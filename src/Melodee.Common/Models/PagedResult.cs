namespace Melodee.Common.Models;

[Serializable]
public sealed record PagedResult<T> : OperationResult<IEnumerable<T>> where T : notnull
{
    public PagedResult(IEnumerable<string>? messages = null)
        : base(messages)
    {
    }

    public int CurrentPage { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}
