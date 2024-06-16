using Melodee.Common.Models;
using Melodee.Common.Utility;
using Melodee.Plugins.Discovery;

namespace Melodee.Plugins.MetaData;

public sealed class CueSheet : MetaDataBase
{
    public override string Id => "3CAB0527-B13F-4C29-97AD-5541229240DD";
    
    public override string DisplayName => nameof(CueSheet);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemInfo fileSystemInfo)
    {
        if (!fileSystemInfo.Exists)
        {
            return false;
        }
        var ext = fileSystemInfo.Extension;
        if (!FileHelper.IsFileMediaMetaDataType(ext))
        {
            return false;
        }
        return string.Equals(ext.Replace(".", ""), "cue");
    }

    public override Task<OperationResult<Release>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

     
        // look at TED.Models.CueSheet ; ps; it's a mess.
        
 // var result = new ConcurrentBag<ATL.Track>();
 //            var isrcCueSheets = Directory.GetFiles(dir, "*.cue");
 //            if (isrcCueSheets.Any())
 //            {
 //                foreach (var isrc in isrcCueSheets)
 //                {
 //                    if (isrc.Contains("isrc.cue", StringComparison.OrdinalIgnoreCase))
 //                    {
 //                        File.SetAttributes(isrc, FileAttributes.Normal);
 //                        File.Delete(isrc);
 //                    }
 //                }
 //            }
 //            var CUEFileForReleaseDirectory = isrcCueSheets.FirstOrDefault();
 //            if (CUEFileForReleaseDirectory != null)
 //            {
 //                ICatalogDataReader theReader = null;
 //                try
 //                {
 //                    theReader = CatalogDataReaderFactory.GetInstance().GetCatalogDataReader(CUEFileForReleaseDirectory);
 //                }
 //                catch (Exception ex)
 //                {
 //                    var throwError = true;
 //                    if(ex.Message.Contains("encoding name"))
 //                    {
 //                       // Encoding wind1252 = Encoding.GetEncoding(1252);
 //                        Encoding wind1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
 //                        byte[] wind1252Bytes = SafeParser.ReadFile(CUEFileForReleaseDirectory);
 //                        byte[] utf8Bytes = Encoding.Convert(wind1252, Encoding.UTF8, wind1252Bytes);
 //                        var newCueFilename = Path.ChangeExtension(CUEFileForReleaseDirectory, "temp");
 //                        await File.WriteAllBytesAsync(newCueFilename, utf8Bytes);
 //                        File.Delete(CUEFileForReleaseDirectory);
 //                        File.Move(newCueFilename, CUEFileForReleaseDirectory);
 //                        try
 //                        {
 //                            theReader = CatalogDataReaderFactory.GetInstance().GetCatalogDataReader(CUEFileForReleaseDirectory);
 //                            throwError = false;                            
 //                        }
 //                        catch (System.Exception ex2)
 //                        {
 //                            logger.LogError("Error reading CUE [{CUEFileForReleaseDirectory}] [{@Error}", CUEFileForReleaseDirectory, ex2);
 //                            return (false, null);
 //                        }
 //                    }
 //                    if(throwError)
 //                    {
 //                        logger.LogError("Error reading CUE [{CUEFileForReleaseDirectory}] [{@Error}", CUEFileForReleaseDirectory, ex);
 //                        return (false, null);
 //                    }
 //                }
 //                if (theReader != null)
 //                {
 //                    var cueSheetParser = new CueSheetParser(CUEFileForReleaseDirectory);
 //                    var cueSheet = cueSheetParser.Parse();
 //                    var splitter = new Cue.CueSheetSplitter(cueSheet, CUEFileForReleaseDirectory, (filePath, mp3FileName, skip, until) =>
 //                    {
 //                        return FFMpegArguments.FromFileInput(filePath)
 //                        .OutputToFile(mp3FileName, true, options =>
 //                        {
 //                            var seekTs = new TimeSpan(0, skip.IndexTime.Minutes, skip.IndexTime.Seconds);
 //                            options.Seek(seekTs);
 //                            if (until != null)
 //                            {
 //                                var untilTs = new TimeSpan(0, until.IndexTime.Minutes, until.IndexTime.Seconds);
 //                                var durationTs = untilTs - seekTs;
 //                                options.WithDuration(durationTs);
 //                            }
 //                            options.WithAudioBitrate(FFMpegCore.Enums.AudioQuality.Ultra);
 //                            options.WithAudioCodec("mp3").ForceFormat("mp3");
 //                        })
 //                        .ProcessAsynchronously(true);
 //                    });
 //                    var splitResults = await splitter.Split();
 //
 //                    var releaseArtist = theReader.Artist ?? throw new Exception("Invalid Artist");
 //                    Parallel.ForEach(splitResults, split =>
 //                    {
 //                        var fileAtl = new ATL.Track(split.FilePath);
 //                        fileAtl.Album = theReader.Title ?? throw new Exception("Invalid Release Title");
 //                        fileAtl.AlbumArtist = releaseArtist;
 //                        fileAtl.Comment = string.Empty;
 //                        fileAtl.DiscNumber = cueSheet.DiscNumber ?? 1;
 //                        fileAtl.DiscTotal = cueSheet.DiscTotal ?? 1;
 //                        var readerTrack = theReader.Tracks.FirstOrDefault(x => x.TrackNumber == split.Track.TrackNum) ?? throw new Exception("Unable to find Track for file");
 //                        fileAtl.Title = readerTrack.Title ?? throw new Exception("Invalid Track Title");
 //                        fileAtl.TrackNumber = readerTrack.TrackNumber;
 //                        fileAtl.TrackTotal = theReader.Tracks.Count();
 //                        fileAtl.Genre = readerTrack.Genre;
 //                        fileAtl.Year = SafeParser.ToDateTime(cueSheet.Date)?.Year ?? CUEFileForReleaseDirectory?.TryToGetYearFromString() ?? throw new Exception("Invalid Release year");
 //                        var trackArtist = readerTrack.Artist.Nullify();
 //                        if (trackArtist != null && !StringExt.DoStringsMatch(releaseArtist, trackArtist))
 //                        {
 //                            fileAtl.Artist = trackArtist;
 //                        }
 //                        else
 //                        {
 //                            fileAtl.Artist = string.Empty;
 //                        }
 //                        if (!fileAtl.Save())
 //                        {
 //                            throw new Exception($"Unable to update metadata for file [{fileAtl.FileInfo().FullName}]");
 //                        }
 //                        result.Add(new ATL.Track(split.FilePath));
 //                    });
 //                    foreach (var cueFile in cueSheet.Files)
 //                    {
 //                        var fn = Path.Combine(dir, cueFile.FileName);
 //                        if (File.Exists(fn))
 //                        {
 //                            File.SetAttributes(fn, FileAttributes.Normal);
 //                            File.Delete(fn);
 //                        }
 //                    }
 //                    File.SetAttributes(CUEFileForReleaseDirectory, FileAttributes.Normal);
 //                    File.Delete(CUEFileForReleaseDirectory);
 //                    return (result.Any(), result);
 //                }
 //            }
 //            return (false, null);        
        
        throw new NotImplementedException();
    }
}