using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Interfaces;

public interface ITagWriter
{
    Task WriteTagsAsync(string filePath, IDictionary<MetaTagIdentifier, object> tags, CancellationToken cancellationToken = default);
    Task WriteTagAsync(string filePath, MetaTagIdentifier tagId, object value, CancellationToken cancellationToken = default);
    Task RemoveTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default);
    Task AddImageAsync(string filePath, AudioImage image, CancellationToken cancellationToken = default);
    Task RemoveImagesAsync(string filePath, CancellationToken cancellationToken = default);
}
