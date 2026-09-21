using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UbisamBase.Core.Backup;
using UbisamBase.Core.Cleaning;
using UbisamBase.Core.DependencyInjection;
using UbisamBase.Core.Localization;
using UbisamBase.Core.Logging;
using UbisamBase.Core.Modules;
using UbisamBase.Core.SettingItems;
using UbisamBase.Core.Settings;
using UbisamBase.Core.Shell;

namespace UbisamBase.Core.Bootstrap;

/// <summary>
/// Shell.exe의 진입점(App.xaml.cs OnStartup)에서 호출하는 부트스트랩 헬퍼.
/// 1) UI 설정(테마 등)을 적용하고
/// 2) 같은 폴더에서 IAppSetup을 구현한 dll을 찾아 로드하고
/// 3) 그 프로젝트가 RegisterViews로 등록한 화면 목록 + 플랫폼 고정 "설정" 탭으로 DI 컨테이너를 구성한 뒤
/// 4) Shell 창을 띄운다.
/// </summary>
public static class UbisamAppBuilder
{
    /// <summary>
    /// 설정 JSON(Settings 폴더)이 저장되는 고정 위치. exe가 어디서 실행되든, 여러 프로그램이
    /// 이 플랫폼으로 만들어져도 항상 D:\UbisamConfig\Settings\ 아래에 모인다. 폴더가 없으면 자동 생성된다.
    /// </summary>
    public const string ConfigRoot = @"D:\UbisamConfig";

    public static IServiceProvider Run(string basePath, Action<IServiceCollection>? configureServices = null)
    {
        var uiSettings = new UiSettingsService();
        uiSettings.Initialize(ConfigRoot);

        LogService.Current.Initialize(ConfigRoot);
        ButtonClickLogger.Install();
        FullscreenHotkeyInstaller.Install();

        var settingsService = new SettingsService(ConfigRoot);

        var languageService = new LanguageService(basePath);
        Application.Current.Resources["Lang"] = languageService;

        var appSetup = AppSetupLoader.Discover(basePath);

        var tabManager = new TabManager();

        // 플랫폼이 항상 제공하는 고정 "설정" 탭을 먼저 만들어둔다 — RegisterViews/RegisterSettings에서
        // AddSub(TabManager.SettingsTabKey, ...)로 이 탭에 바로 서브탭을 추가할 수 있게 하기 위해서다.
        // Order가 항상 최대값으로 고정되므로 등록 순서와 무관하게 탭은 마지막에 표시된다.
        tabManager.AddFixedMain<SettingsHomeView, SettingsHomeViewModel>(Icon.Settings, TabManager.SettingsTabKey, "설정");

        appSetup.RegisterViews(tabManager);
        appSetup.RegisterSettings(tabManager);

        tabManager.AddSub<UiSettingsView, UiSettingsViewModel>(TabManager.SettingsTabKey, "UI Setting");
        tabManager.AddSub<AllSettingsView, AllSettingsViewModel>(TabManager.SettingsTabKey, "설정값");
        tabManager.AddSub<BackupView, BackupViewModel>(TabManager.SettingsTabKey, "백업");
        tabManager.AddSub<CleanerView, CleanerViewModel>(TabManager.SettingsTabKey, "클리너");
        tabManager.AddSub<LogViewerView, LogViewerViewModel>(TabManager.SettingsTabKey, "로그");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddUbisamBaseCore(configuration);
        services.AddSingleton(appSetup);
        services.AddSingleton(tabManager);
        services.AddSingleton(uiSettings);
        services.AddSingleton<ISettingsService>(settingsService);
        services.AddSingleton(languageService);
        appSetup.RegisterServices(services);
        configureServices?.Invoke(services);

        var provider = services.BuildServiceProvider();

        var shell = provider.GetRequiredService<ShellWindow>();
        Application.Current.MainWindow = shell;
        shell.Show();

        return provider;
    }
}
