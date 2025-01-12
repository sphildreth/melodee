namespace Melodee.Common.Configuration;

public interface IMelodeeConfigurationFactory
{
    public void Reset();

    public Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
}
