using Microsoft.Extensions.Configuration;

namespace UbisamBase.Core.Services;

public class AppConfigService : IAppConfigService
{
    public IConfiguration Configuration { get; }

    public AppConfigService(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public T GetSection<T>(string sectionName) where T : new()
    {
        var result = new T();
        Configuration.GetSection(sectionName).Bind(result);
        return result;
    }
}
