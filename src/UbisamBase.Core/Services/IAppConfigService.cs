using Microsoft.Extensions.Configuration;

namespace UbisamBase.Core.Services;

/// <summary>appsettings.json 기반 설정 접근을 위한 공통 서비스.</summary>
public interface IAppConfigService
{
    IConfiguration Configuration { get; }

    /// <summary>지정한 섹션을 T 타입 객체로 바인딩해서 반환한다.</summary>
    T GetSection<T>(string sectionName) where T : new();
}
