using Melodee.Common.Enums;

namespace Melodee.Common.Metadata.AudioTags.Interfaces;

public interface IMediaAudioReader
{
    Task<IDictionary<MediaAudioIdentifier, object>> ReadMediaAudiosAsync(string filePath, CancellationToken cancellationToken = default);
}
