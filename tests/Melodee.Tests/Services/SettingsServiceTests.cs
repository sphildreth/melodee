using Melodee.Common.Constants;
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
        Assert.NotNull(listResult);
        Assert.True(listResult.IsSuccess);         
        Assert.NotEmpty(listResult.Data);
        Assert.Contains(listResult.Data, x => x.Key == SettingRegistry.ValidationMaximumSongNumber);
        Assert.Equal(1, listResult.TotalPages);
    }
}
