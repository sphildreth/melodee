using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;

namespace Melodee.Common.Services.Interfaces;

public interface ISettingService
{
    Task<Dictionary<string, object?>> GetAllSettingsAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Setting>> ListAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    Task<OperationResult<T?>> GetValueAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default);

    Task<OperationResult<Setting?>> GetAsync(string key, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> SetAsync(string key, string value, CancellationToken cancellationToken = default);

    Task<IMelodeeConfiguration> GetMelodeeConfigurationAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> UpdateAsync(Setting detailToUpdate, CancellationToken cancellationToken = default);
}
