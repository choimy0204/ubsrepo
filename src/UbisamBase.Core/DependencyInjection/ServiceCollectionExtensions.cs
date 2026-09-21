using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UbisamBase.Core.Backup;
using UbisamBase.Core.Cleaning;
using UbisamBase.Core.Messaging;
using UbisamBase.Core.Services;
using UbisamBase.Core.Shell;
using UbisamBase.Core.Utils;

namespace UbisamBase.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>Shell 화면과 공통 서비스(설정, 백업, 클리너, 이벤트버스, 타임아웃 관리 등)를 DI 컨테이너에 등록한다.</summary>
    public static IServiceCollection AddUbisamBaseCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<TimeOutManager>();
        services.AddSingleton<BackupService>();
        services.AddSingleton<CleanerService>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<ShellWindow>();
        return services;
    }
}
