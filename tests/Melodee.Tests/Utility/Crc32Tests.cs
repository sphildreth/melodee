using Melodee.Common.Utility;

namespace Melodee.Tests.Utility;

public class Crc32Tests
{
    [Fact]
    public void ComputeAndCompareCrc32Hash()
    {
        var mp3File = @"/melodee_test/tests/test.mp3";
        var fileInfo = new FileInfo(mp3File);
        if (fileInfo.Exists)
        {
            var crc = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc);
            
            var crc2 = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc2);
            Assert.Equal(crc, crc2);
            
            fileInfo.LastWriteTime = fileInfo.LastWriteTime.AddHours(1);
            var crc3 = Crc32.Calculate(fileInfo);
            Assert.NotNull(crc3);
            Assert.Equal(crc, crc3);
        }
    }

}
