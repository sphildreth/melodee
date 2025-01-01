using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;

namespace Melodee.Tests.Services;

public sealed class SettingsServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterOnIdValueEqualsAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var idValue = 0;
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            idValue = context.Settings.First(x => x.Key == SettingRegistry.ValidationMaximumSongNumber).Id;
        }

        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy = new[]
            {
                new FilterOperatorInfo(nameof(Setting.Id), FilterOperator.Equals, idValue)
            },
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterOnKeyValueEqualsAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy = new[]
            {
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Equals, SettingRegistry.ValidationMaximumSongNumber)
            },
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task ListWithFilterLikeAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            FilterBy = new[]
            {
                new FilterOperatorInfo(nameof(Setting.Key), FilterOperator.Contains, "bit")
            },
            PageSize = 1
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ConversionBitrate);
        Assert.Equal(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages); }

    [Fact]
    public async Task ListWithSortAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var listResult = await service.ListAsync(new PagedRequest
        {
            OrderBy = new Dictionary<string, string>
            {
                { nameof(Setting.Id), PagedRequest.OrderAscDirection }
            },
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.Equal(1, listResult.Data.First().Id);
        Assert.NotEqual(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);

        listResult = await service.ListAsync(new PagedRequest
        {
            OrderBy = new Dictionary<string, string>
            {
                { nameof(Setting.Id), PagedRequest.OrderDescDirection }
            },
            PageSize = 1000
        });
        AssertResultIsSuccessful(listResult);
        Assert.NotEqual(1, listResult.Data.First().Id);
        Assert.NotEqual(1, listResult.TotalCount);
        Assert.Equal(1, listResult.TotalPages);
    }

    [Fact]
    public async Task GetSettingByKeyAndValueAsync()
    {
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var getResult = await service.GetAsync(SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getResult);

        var getIntValueResult = await service.GetValueAsync<int>(SettingRegistry.ValidationMaximumSongNumber);
        AssertResultIsSuccessful(getIntValueResult);
        Assert.IsType<int>(getIntValueResult.Data);
        Assert.True(getIntValueResult.Data > 0);

        var getStringValueResult = await service.GetValueAsync<string>(SettingRegistry.ProcessingSongTitleRemovals);
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
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var listResult = await service.GetAllSettingsAsync();
        Assert.NotEmpty(listResult);
        Assert.Contains(listResult, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Contains(listResult, x => x.Key == SettingRegistry.ProcessingMaximumProcessingCount);
    }

    [Fact]
    public async Task GetSettingWithFunc()
    {
        var shouldBeValueInt = 99;
        var service = new SettingService(Logger, CacheManager, MockFactory());
        var configuration = await service.GetMelodeeConfigurationAsync();
        Assert.NotEmpty(configuration.Configuration);
        
        var maxSongsToProcess = configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
        Assert.Equal(0, maxSongsToProcess);

        configuration.SetSetting(SettingRegistry.ProcessingMaximumProcessingCount, shouldBeValueInt);
        var getIntValueResult = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount);
        Assert.Equal(shouldBeValueInt, getIntValueResult);

        configuration.SetSetting(SettingRegistry.ProcessingMaximumProcessingCount, 15);
        getIntValueResult = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => int.MaxValue);
        Assert.NotEqual(shouldBeValueInt, getIntValueResult);
    }
}
