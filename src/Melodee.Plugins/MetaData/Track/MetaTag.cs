using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Track.Extensions;
using Serilog;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Track;

/* Class Notes;

ID3 Tags are tricky and vary across vendor and implementation. This attempts to normalize tags across these varying sources.

Some reference sites:
 * https://github.com/Zeugma440/atldotnet/blob/main/ATL/AudioData/IO/ID3v2.cs
 * https://wiki.hydrogenaud.io/index.php?title=Tag_Mapping
 * https://exiftool.org/TagNames/ID3.html
 * https://mutagen-specs.readthedocs.io/en/latest/id3/id3v2.3.0.html
 * https://www.mediamonkey.com/sw/webhelp/frame/index.html?abouttrackproperties.htm
 * https://eyed3.readthedocs.io/en/latest/compliance.html
 * https://www.theoplayer.com/docs/theoplayer/v7/api-reference/web/interfaces/ID3UserDefinedUrlLink.html

 All v2.4 dates follow ISO 8601 formats.

*/

public sealed class MetaTag(Configuration configuration) : MetaDataBase(configuration), ITrackPlugin
{
    public override string Id => "0F622E4B-64CD-4033-8B23-BA2001F045FA";

    public override string DisplayName => nameof(MetaTag);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists())
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension());
    }

    public Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemFileInfo fileSystemFileInfo, CancellationToken cancellationToken = default)
    {
        using (Operation.Time("[{PluginName}] Processing [{FileSystemFileInfo}]", DisplayName, fileSystemFileInfo.FullName()))
        {
            var tags = new List<MetaTag<object>>();
            var mediaAudios = new List<MediaAudio<object>>();
            var images = new List<ImageInfo>();

            try
            {
                if (fileSystemFileInfo.Exists())
                {
                    var fileAtl = new ATL.Track(fileSystemFileInfo.FullName());
                    if (!fileAtl.MetadataFormats.Any(x => x.ID < 0) && IsAtlTrackForMp3(fileAtl))
                    {
                        var atlDictionary = fileAtl.ToDictionary();

                        var metaAudioIdentifierDictionary = MediaAudioIdentifier.NotSet.ToDictionary();
                        foreach (var metaTagIdentifier in metaAudioIdentifierDictionary)
                        {
                            if (atlDictionary.TryGetValue(metaTagIdentifier.Value, out var v))
                            {
                                if (v is string s)
                                {
                                    v = s.Nullify();
                                }

                                if (v != null)
                                {
                                    var identifier = SafeParser.ToEnum<MediaAudioIdentifier>(metaTagIdentifier.Key);
                                    mediaAudios.Add(new MediaAudio<object>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }

                        if (fileAtl.IsVBR)
                        {
                            mediaAudios.Add(new MediaAudio<object>
                            {
                                Identifier = MediaAudioIdentifier.IsVbr,
                                Value = true
                            });
                        }

                        if (fileAtl.ChannelsArrangement != null)
                        {
                            mediaAudios.Add(new MediaAudio<object>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementDescription,
                                Value = fileAtl.ChannelsArrangement.Description
                            });
                            mediaAudios.Add(new MediaAudio<object>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementNumberChannels,
                                Value = fileAtl.ChannelsArrangement.NbChannels
                            });
                        }

                        if (fileAtl.TechnicalInformation != null)
                        {
                            mediaAudios.Add(new MediaAudio<object>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataOffset,
                                Value = fileAtl.TechnicalInformation.AudioDataOffset
                            });
                            mediaAudios.Add(new MediaAudio<object>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataSize,
                                Value = fileAtl.TechnicalInformation.AudioDataSize
                            });
                        }

                        if (fileAtl.EmbeddedPictures.Any() && Configuration.PluginProcessOptions.DoLoadEmbeddedImages)
                        {
                            foreach (var embeddedPicture in fileAtl.EmbeddedPictures)
                            {
                                var imageInfo = SixLabors.ImageSharp.Image.Load(embeddedPicture.PictureData);
                                images.Add(new ImageInfo
                                {
                                    PictureIdentifier = SafeParser.ToEnum<PictureIdentifier>(embeddedPicture.PicType),
                                    Bytes = embeddedPicture.PictureData,
                                    Width = imageInfo.Width,
                                    Height = imageInfo.Height,
                                    SortOrder = embeddedPicture.Position
                                });
                            }
                        }

                        var metaTagIdentifierDictionary = MetaTagIdentifier.NotSet.ToDictionary();
                        foreach (var metaTagIdentifier in metaTagIdentifierDictionary)
                        {
                            if (atlDictionary.TryGetValue(metaTagIdentifier.Value, out var v))
                            {
                                if (v is string s)
                                {
                                    v = s.Nullify();
                                }

                                if (v is DateTime vDt)
                                {
                                    v = vDt > DateTime.MinValue && vDt < DateTime.MaxValue ? v : null;
                                }

                                if (v != null)
                                {
                                    var identifier = SafeParser.ToEnum<MetaTagIdentifier>(metaTagIdentifier.Key);
                                    if (metaTagIdentifier.Key == (int)MetaTagIdentifier.Date ||
                                        metaTagIdentifier.Key == (int)MetaTagIdentifier.RecordingDate)
                                    {
                                        var dt = SafeParser.ToDateTime(v);
                                        if (dt.HasValue)
                                        {
                                            tags.Add(new MetaTag<object>
                                            {
                                                Identifier = MetaTagIdentifier.OrigReleaseYear,
                                                Value = dt.Value.Year
                                            });
                                        }
                                    }

                                    tags.Add(new MetaTag<object>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", fileSystemFileInfo);
            }
            return Task.FromResult(new OperationResult<Common.Models.Track>
            {
                Data = new Common.Models.Track
                {
                    File = fileSystemFileInfo,
                    Images = images,
                    Tags = tags,
                    MediaAudios = mediaAudios,
                    SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
                }
            });            
        }
    }

    private static bool IsAtlTrackForMp3(ATL.Track track)
    {
        if (track.AudioFormat?.ShortName == null)
        {
            return false;
        }

        if (string.Equals(track.AudioFormat?.ShortName, "mpeg-4", StringComparison.OrdinalIgnoreCase))
        {
            var ext = track.FileInfo().Extension;
            if (!ext.ToLower().EndsWith("m4a")) // M4A is an audio file using the MP4 encoding
            {
                Console.WriteLine($"Video file found in Scanning. File [{track.FileInfo().FullName}]");
                return false;
            }
        }

        return track is { AudioFormat: { ID: > -1 }, Duration: > 0 };
    }


    // // Fields allowed to have multiple values according to ID3v2.2-3 specs
    // private static readonly ISet<string> multipleValuev23Fields = new HashSet<string> { "TP1", "TCM", "TXT", "TLA", "TOA", "TOL", "TCOM", "TEXT", "TOLY", "TOPE", "TPE1" };
    //
}