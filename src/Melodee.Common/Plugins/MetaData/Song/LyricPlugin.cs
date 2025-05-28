using ATL;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Extensions;
using Melodee.Common.Serialization;

namespace Melodee.Common.Plugins.MetaData.Song;

public class LyricPlugin(ISerializer serializer, IMelodeeConfigurationFactory configurationFactory): ILyricPlugin
{
    public string Id => "130C1EC9-D04D-4F22-BCBD-17791649CEF7";

    public string DisplayName => nameof(LyricPlugin);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;
    
    public bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return false;
    }

    public async Task<OperationResult<Lyrics?>> GetLyricsAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken token = default)
    {
        Lyrics? result = null;
        var configuration = await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);
        var lyricFilesEnabled = configuration.GetValue<bool>(SettingRegistry.LyricFilesEnabled);
        if (lyricFilesEnabled)
        {
            var lyricsFileName = Path.Combine(directoryInfo.Path, $"{fileSystemInfo.Name}.txt");
            if (File.Exists(lyricsFileName))
            {
                var lyricsFileContent = await File.ReadAllTextAsync(lyricsFileName, token).ConfigureAwait(false);
                result = serializer.Deserialize<Lyrics>(lyricsFileContent);      
            }
            else
            {
                var lyricsList = await GetLyricListAsync(directoryInfo, fileSystemInfo, token).ConfigureAwait(false);
                if(lyricsList is { IsSuccess: true, Data: not null })
                {
                    result = lyricsList.Data.ToLyrics();
                }
            }
        }

        if (result == null)
        {
            if (fileSystemInfo.Exists(directoryInfo))
            {
                var fileAtl = new Track(fileSystemInfo.FullName(directoryInfo));
                var ult = fileAtl.Lyrics.UnsynchronizedLyrics.NormalizeLineEndings();
                if (ult.Nullify() != null)
                {
                    result = new Lyrics
                    {
                        Artist = fileAtl.AlbumArtist ?? fileAtl.Artist,
                        Title = fileAtl.Title,
                        Value = ult!
                    };                        
                }
            }
        }

        return new OperationResult<Lyrics?>
        {
            Data = result
        };
    }

    public async Task<OperationResult<LyricsList?>> GetLyricListAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken token = default)
    {
        LyricsList? result = null; 
        var configuration = await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);
        var lyricFilesEnabled = configuration.GetValue<bool>(SettingRegistry.LyricFilesEnabled);
        if (lyricFilesEnabled)
        {
            var lyricsListFileName = Path.Combine(directoryInfo.Path, $"{fileSystemInfo.Name}.lrc");
            if (File.Exists(lyricsListFileName))
            {
                var lyricsListFileContent = await File.ReadAllTextAsync(lyricsListFileName, token).ConfigureAwait(false);
                var lyricsList = serializer.Deserialize<LyricsList>(lyricsListFileContent);
                result = lyricsList;
            }
        }
        if (result == null)
        {
            if (fileSystemInfo.Exists(directoryInfo))
            {
                var fileAtl = new Track(fileSystemInfo.FullName(directoryInfo));
                var slt = fileAtl.Lyrics.SynchronizedLyrics;
                if (slt.Count > 0)
                {
                    result = new LyricsList
                    {
                        DisplayArtist = fileAtl.AlbumArtist ?? fileAtl.Artist,
                        DisplayTitle = fileAtl.Title,
                        Lang = "xxx",
                        Synced = true,
                        Line = slt.Select(x => new LyricsListLine(x.Text, x.TimestampMs)).ToArray()
                    };
                }
            }
        }
        return new OperationResult<LyricsList?>
        {
            Data = result
        };
    }
}
