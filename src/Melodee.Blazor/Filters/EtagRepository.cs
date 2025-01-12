using System.Collections.Concurrent;

namespace Melodee.Blazor.Filters;

public class EtagRepository
{
    private readonly ConcurrentDictionary<string, string> _eTags = new();

    public bool AddEtag(string? apiKeyId, string? etag)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyId) || !string.IsNullOrWhiteSpace(etag))
        {
            return _eTags.TryAdd(apiKeyId!, etag!);
        }

        return false;
    }

    public bool EtagMatch(string? apiKeyId, string? etag)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyId) || !string.IsNullOrWhiteSpace(etag))
        {
            return _eTags.ContainsKey(apiKeyId!) && _eTags[apiKeyId!] == etag;
        }

        return false;
    }
}
