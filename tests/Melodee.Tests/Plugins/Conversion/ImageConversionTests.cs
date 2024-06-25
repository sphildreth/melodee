using Melodee.Common.Models.Configuration;
using Melodee.Plugins;

namespace Melodee.Tests.Plugins.Conversion;

public class ImageConversionTests
{
    
    
    [Fact]
    public async Task ValidateConvertingPngToJpgAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testpng.png";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName);
            Assert.True(convertedFileInfo.Exists);

        }
    }
    
    [Fact]
    public async Task ValidateConvertingGifToJpgAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testgif.gif";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName);
            Assert.True(convertedFileInfo.Exists);

        }
    }    
    
    [Fact]
    public async Task ValidateConvertingTiffToJpgAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testtiff.tiff";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName);
            Assert.True(convertedFileInfo.Exists);

        }
    }      
    
    [Fact]
    public async Task ValidateConvertingWebpJpgAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testwebp.webp";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName);
            Assert.True(convertedFileInfo.Exists);

        }
    }      
    
    [Fact]
    public async Task ValidateConvertingBitmapJpgAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testbmp.bmp";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName);
            Assert.True(convertedFileInfo.Exists);

        }
    }      
    
    [Fact]
    public async Task ValidateConvertingNonImageFailsAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test.mp3";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Image.ImageConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo);
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
        }
    }      
}