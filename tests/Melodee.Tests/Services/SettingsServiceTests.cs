using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using Microsoft.EntityFrameworkCore.Design;

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
}
