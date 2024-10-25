using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;

namespace Melodee.Tests.Services;

public sealed class SettingsServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListAsync()
    {
        var service = GetSettingService();
        var listResult = await service.ListAsync(ServiceUser.Instance.Value, new PagedRequest
        {
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task GetSettingByKeyAndValueAsync()
    {
        var service = GetSettingService();
        var getResult = await service.GetAsync(ServiceUser.Instance.Value, SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getResult);
        
        var getIntValueResult = await service.GetValueAsync<int>(ServiceUser.Instance.Value, SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getIntValueResult);
        Assert.IsType<int>(getIntValueResult.Data);
        Assert.True(getIntValueResult.Data > 0);
        
        var getStringValueResult = await service.GetValueAsync<string>(ServiceUser.Instance.Value, SettingRegistry.ProcessingSongTitleRemovals);
        AssertResultIsSuccessful(getStringValueResult);
        Assert.IsType<string>(getStringValueResult.Data);
        Assert.NotNull(getStringValueResult.Data.Nullify());
    }

    [Fact]
    public void GetSettingSetAndConvert()
    {
        var settings = SettingService.AllSettings();
        Assert.NotNull(settings);
        Assert.Contains(settings, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);

        var shouldBeValueInt = 99;
        SettingService.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueInt);
        Assert.Equal(shouldBeValueInt, settings[SettingRegistry.ValidationMaximumSongNumber]);
        
        var shouldBeValueBool = true;
        SettingService.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueBool);
        Assert.Equal(shouldBeValueBool, settings[SettingRegistry.ValidationMaximumSongNumber]); 
        Assert.True(SettingService.IsTrue(settings, SettingRegistry.ValidationMaximumSongNumber));
    }

    [Fact]
    public async Task GetAllSettingsAsync()
    {
        var service = GetSettingService();
        var listResult = await service.GetAllSettingsAsync();
        Assert.NotEmpty(listResult);
        Assert.Contains(listResult, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
    }
}
