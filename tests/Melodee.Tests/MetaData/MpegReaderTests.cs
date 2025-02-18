using Melodee.Common.Metadata.Mpeg;

namespace Melodee.Tests.MetaData;

public class MpegReaderTests
{
    [Fact]
    public async Task ReadValidMp3()
    {
        var testFile = @"/melodee_test/tests/test.mp3";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.True(mpeg.IsValid);
        }
    }
    
    [Fact]
    public async Task  ReadValidMp32()
    {
        var testFile = @"/melodee_test/tests/test2.mp3";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.True(mpeg.IsValid);
        }
    }    
    
    [Fact]
    public async Task  ReadValidTest6()
    {
        var testFile = @"/melodee_test/tests/test6.mp3";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.True(mpeg.IsValid);
        }
    }        
    
    [Fact]
    public async Task  ReadTestMultiChannel()
    {
        var testFile = @"/melodee_test/tests/testmultichannel.flac";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.False(mpeg.IsValid);
        }
    }           
    
    [Fact]
    public async Task  ReadValidConvertedFromFlacMp32()
    {
        var testFile = @"/melodee_test/tests/testconvertedfromflac.mp3";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.True(mpeg.IsValid);
        }
    }      
    
    [Fact]
    public async Task  ReadValidMp3Mpeg25()
    {
        var testFile = @"/melodee_test/tests/testmpeg-2-5.mp3";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.True(mpeg.IsValid);

            var idv2 = new Id3V2(fileInfo.FullName);
            idv2.Read();
            Assert.NotNull(idv2.Title);
            Assert.NotNull(idv2.Artist);
            Assert.NotNull(idv2.Album);
            Assert.NotNull(idv2.Year);
            Assert.NotNull(idv2.Track);
        }
    }        
    
    
    [Fact]
    public async Task  ReadWav()
    {
        var testFile = @"/melodee_test/tests/test.wav";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.False(mpeg.IsValid);
        }
    }       
    
    [Fact]
    public async Task  ReadOgg()
    {
        var testFile = @"/melodee_test/tests/test.ogg";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.False(mpeg.IsValid);
        }
    }      
    
    [Fact]
    public async Task  ReadM4a()
    {
        var testFile = @"/melodee_test/tests/test.m4a";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.False(mpeg.IsValid);
        }
    }  
    
    [Fact]
    public async Task  ReadFlac()
    {
        var testFile = @"/melodee_test/tests/testflac.flac";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync();
            Assert.NotNull(mpeg.Bitrate);
            Assert.False(mpeg.IsValid);
        }
    }      
       
}
