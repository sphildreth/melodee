using FFMpegCore;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Track.Extensions;
using Melodee.Plugins.Processor;
using Serilog;
using Serilog.Events;
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

public sealed class MetaTag(IMetaTagsProcessorPlugin metaTagsProcessorPlugin, Configuration configuration) : MetaDataBase(configuration), ITrackPlugin
{
    private readonly IMetaTagsProcessorPlugin _metaTagsProcessorPlugin = metaTagsProcessorPlugin;
    public override string Id => "0F622E4B-64CD-4033-8B23-BA2001F045FA";

    public override string DisplayName => nameof(MetaTag);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo))
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("[{PluginName}] Processing [{FileSystemFileInfo}]", DisplayName, fileSystemFileInfo.Name))
        {
            var tags = new List<MetaTag<object?>>();
            var mediaAudios = new List<MediaAudio<object?>>();
            var images = new List<ImageInfo>();

            try
            {
                if (fileSystemFileInfo.Exists(directoryInfo))
                {
                    var fileAtl = new ATL.Track(fileSystemFileInfo.FullName(directoryInfo));
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
                                    mediaAudios.Add(new MediaAudio<object?>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }

                        IMediaAnalysis? ffProbeMediaAnalysis = null;
                        try
                        {
                            ffProbeMediaAnalysis = await FFProbe.AnalyseAsync(fileSystemFileInfo.FullName(directoryInfo), cancellationToken: cancellationToken);

                            if (ffProbeMediaAnalysis.PrimaryAudioStream != null)
                            {   
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.CodecLongName,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.CodecLongName
                                });
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.Channels,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.Channels
                                });
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.ChannelLayout,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.ChannelLayout
                                });     
                            }              
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", fileSystemFileInfo);
                        }

                        if (fileAtl.IsVBR)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.IsVbr,
                                Value = true
                            });
                        }

                        if (fileAtl.ChannelsArrangement != null)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementDescription,
                                Value = fileAtl.ChannelsArrangement.Description
                            });
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementNumberChannels,
                                Value = fileAtl.ChannelsArrangement.NbChannels
                            });
                        }

                        if (fileAtl.TechnicalInformation != null)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataOffset,
                                Value = fileAtl.TechnicalInformation.AudioDataOffset
                            });
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataSize,
                                Value = fileAtl.TechnicalInformation.AudioDataSize
                            });
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
                                        metaTagIdentifier.Key == (int)MetaTagIdentifier.OrigReleaseDate ||
                                        metaTagIdentifier.Key == (int)MetaTagIdentifier.RecordingDate)
                                    {
                                        var dt = SafeParser.ToDateTime(v);
                                        if (dt.HasValue)
                                        {
                                            tags.Add(new MetaTag<object?>
                                            {
                                                Identifier = MetaTagIdentifier.OrigReleaseYear,
                                                Value = dt.Value.Year
                                            });
                                        }
                                    }

                                    tags.Add(new MetaTag<object?>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }
                        
                        var adData1 = fileAtl.AdditionalFields.ToDictionary();
                        var adData2 = ffProbeMediaAnalysis?.Format.Tags?.ToDictionary() ?? new Dictionary<string, string>();
                        var additionalTags = MetaTagsForTagDictionary(DictionaryExtensions.Merge(new [] { adData1, adData2 }));
                        foreach (var additionalTag in additionalTags)
                        {
                            if (tags.All(x => x.Identifier != additionalTag.Identifier))
                            {
                                tags.Add(additionalTag);
                            }
                        }
                        
                        if (fileAtl.EmbeddedPictures.Any() && Configuration.PluginProcessOptions.DoLoadEmbeddedImages)
                        {
                            var releaseId = SafeParser.Hash(
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist)?.Value?.ToString() ?? string.Empty,
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigReleaseYear)?.Value?.ToString() ?? 
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.RecordingYear)?.Value?.ToString() ?? 
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.RecordingDateOrYear)?.Value?.ToString() ?? string.Empty,
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album)?.Value?.ToString() ?? string.Empty);
                            var pictureIndex = 0;
                            foreach (var embeddedPicture in fileAtl.EmbeddedPictures)
                            {
                                var imageInfo = SixLabors.ImageSharp.Image.Load(embeddedPicture.PictureData);
                                var imageCrcHash = CRC32.Calculate(embeddedPicture.PictureData);
                                if (directoryInfo.GetFileForCrcHash("jpg", imageCrcHash) == null)
                                {
                                    var pictureIdentifier = SafeParser.ToEnum<PictureIdentifier>(embeddedPicture.PicType);
                                    var newImageFileName = Path.Combine(directoryInfo.Path, $"{releaseId}-{(pictureIndex + 1).ToStringPadLeft(2)}-{embeddedPicture.PicType.ToString()}.jpg");
                                    await File.WriteAllBytesAsync(newImageFileName, embeddedPicture.PictureData, cancellationToken);
                                    images.Add(new ImageInfo
                                    {
                                        CrcHash = imageCrcHash,
                                        PictureIdentifier = pictureIdentifier,
                                        FileInfo = (new FileInfo(newImageFileName).ToFileSystemInfo()),
                                        Width = imageInfo.Width,
                                        Height = imageInfo.Height,
                                        SortOrder = embeddedPicture.Position,
                                        WasEmbeddedInTrack = true
                                    });
                                }

                                pictureIndex++;
                            }
                        }                            
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", fileSystemFileInfo);
            }

            // Ensure that OrigReleaseYear exists and if not add with invalid date (will get set later by MetaTagProcessor.)
            if (tags.All(x => x.Identifier != MetaTagIdentifier.OrigReleaseYear))
            {
                tags.Add(new MetaTag<object?>{
                    Identifier = MetaTagIdentifier.OrigReleaseYear,
                    Value = 0
                });
            }

            var metaTagsProcessorResult = await _metaTagsProcessorPlugin.ProcessMetaTagAsync(directoryInfo, fileSystemFileInfo, tags, cancellationToken);
            if (!metaTagsProcessorResult.IsSuccess)
            {
                return new OperationResult<Common.Models.Track>(metaTagsProcessorResult.Messages)
                {
                    Errors = metaTagsProcessorResult.Errors,
                    Data = new Common.Models.Track
                    {
                        CrcHash = string.Empty,
                        File = fileSystemFileInfo
                    }
                };
            }
            var track = new Common.Models.Track
            {
                CrcHash = CRC32.Calculate(new FileInfo(fileSystemFileInfo.FullName(directoryInfo))),
                File = fileSystemFileInfo,
                Images = images,
                Tags = metaTagsProcessorResult.Data,
                MediaAudios = mediaAudios,
                SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
            };
            return new OperationResult<Common.Models.Track>
            {
                Data = track 
            };            
        }
    }

    private IEnumerable<MetaTag<object?>> MetaTagsForTagDictionary(Dictionary<string, string> tagsDictionary)
    {
        var result = new List<MetaTag<object?>>();
        if (tagsDictionary.Count == 0)
        {
            return result;
        }

        foreach (var kp in tagsDictionary)
        {
            var k = kp.Key.CleanString()?.Replace(" ", string.Empty).ToUpperInvariant();
            switch (k)
            {
                case "ARTISTS":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.Artists))
                    {
                        var artists = kp.Value.ToCleanedMultipleArtistsValue();
                        if (artists != null)
                        {
                            result.Add(new MetaTag<object?>()
                            {
                                Identifier = MetaTagIdentifier.Artists,
                                Value = artists
                            });
                        }
                    }
                    break;
                
                case "LENGTH":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.Length))
                    {
                        result.Add(new MetaTag<object?>()
                        {
                            Identifier = MetaTagIdentifier.Length,
                            Value = kp.Value
                        });
                    }
                    break;    
                
                case "DATE":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.OrigReleaseDate))
                    {
                        result.Add(new MetaTag<object?>()
                        {
                            Identifier = MetaTagIdentifier.OrigReleaseDate,
                            Value = kp.Value
                        });
                    }
                    break;          
                
                case "TRACK":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.TrackNumberTotal))
                    {
                        if (kp.Value.Contains('/'))
                        {
                            result.Add(new MetaTag<object?>()
                            {
                                Identifier = MetaTagIdentifier.TrackNumberTotal,
                                Value = kp.Value
                            });
                        }
                    }
                    break;                  
                
                // ReSharper disable once StringLiteralTypo
                case "WXXX":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.UserDefinedUrlLink))
                    {
                        result.Add(new MetaTag<object?>()
                        {
                            Identifier = MetaTagIdentifier.UserDefinedUrlLink,
                            Value = kp.Value
                        });
                    }
                    break;                  
                
            }
        }

        return result;
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
}