using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;

namespace Melodee.Services.Interfaces;

public interface ISettingService
{
    Task<Dictionary<string, object?>> GetAllSettingsAsync(CancellationToken cancellationToken = default);
    
    Task<Common.Models.PagedResult<Setting>> ListAsync(Common.Models.PagedRequest pagedRequest, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<T?>> GetValueAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Setting?>> GetAsync(string key, CancellationToken cancellationToken = default);
    
    Task<IMelodeeConfiguration> GetMelodeeConfigurationAsync(CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<bool>> UpdateAsync(Setting detailToUpdate, CancellationToken cancellationToken = default);
}
