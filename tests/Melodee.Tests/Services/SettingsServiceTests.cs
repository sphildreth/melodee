using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Services;

namespace Melodee.Tests.Services;

public sealed class SettingsServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListAsync()
    {
        var service = GetSettingService();
        var listResult = await service.ListAsync( new PagedRequest
        {
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalPages);
    }
    
    [Fact]
    public async Task ListWithFilterAndSortAsync()
    {
        var service = GetSettingService();
        var listResult = await service.ListAsync( new PagedRequest
        {
            FilterBy = new []
            {
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Equals, $"\"{ SettingRegistry.ValidationMaximumSongNumber }\""),
            },
            OrderBy = new Dictionary<string, string>
            {
                [nameof(Setting.Key)] = "desc"
            },
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }    

    [Fact]
    public async Task GetSettingByKeyAndValueAsync()
    {
        var service = GetSettingService();
        var getResult = await service.GetAsync( SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getResult);
        
        var getIntValueResult = await service.GetValueAsync<int>( SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getIntValueResult);
        Assert.IsType<int>(getIntValueResult.Data);
        Assert.True(getIntValueResult.Data > 0);
        
        var getStringValueResult = await service.GetValueAsync<string>( SettingRegistry.ProcessingSongTitleRemovals);
        AssertResultIsSuccessful(getStringValueResult);
        Assert.IsType<string>(getStringValueResult.Data);
        Assert.NotNull(getStringValueResult.Data.Nullify());
    }

    [Fact]
    public void GetSettingSetAndConvert()
    {
        var settings = MelodeeConfiguration.AllSettings();
        Assert.NotNull(settings);
        Assert.Contains(settings, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);

        var shouldBeValueInt = 99;
        MelodeeConfiguration.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueInt);
        Assert.Equal(shouldBeValueInt, settings[SettingRegistry.ValidationMaximumSongNumber]);
        
        var shouldBeValueBool = true;
        MelodeeConfiguration.SetSetting(settings, SettingRegistry.ValidationMaximumSongNumber, shouldBeValueBool);
        Assert.Equal(shouldBeValueBool, settings[SettingRegistry.ValidationMaximumSongNumber]); 
        Assert.True(MelodeeConfiguration.IsTrue(settings, SettingRegistry.ValidationMaximumSongNumber));
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
