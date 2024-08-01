namespace Melodee.Common.Models;

[Serializable]
public sealed record PagedResult<T> : OperationResult<IEnumerable<T>> where T : notnull
{
    public int CurrentPage { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }

    public PagedResult(IEnumerable<string>? messages = null)
        : base(messages)
    {
    }
}