using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UbisamBase.Core.SettingItems;

namespace UbisamBase.Core.Modules;

/// <summary>
/// ITabManager의 기본 구현체. Shell 부트스트랩 과정에서 하나 만들어져
/// IAppSetup.RegisterViews(manager)로 전달되고, 등록이 끝나면
/// Shell이 <see cref="Registrations"/>를 읽어 화면을 구성한다.
/// </summary>
public sealed class TabManager : ITabManager
{
    /// <summary>플랫폼 고정 "설정" 메인탭의 key. AddSettingsSub/AddSub의 parentKey로 쓴다.</summary>
    public const string SettingsTabKey = "__UbisamSettings";

    // 앱 실행 중 TabManager는 항상 하나뿐이다(UbisamAppBuilder.Run에서 한 번만 생성) — 그 인스턴스를
    // 여기 담아두면, Shell UI와 직접 연결되지 않은 코드(플러그인 dll 등)도 정적으로
    // TabManager.Activate(key)를 호출해 화면을 전환할 수 있다.
    private static TabManager? current;

    private readonly List<MainTabRegistration> mainTabs = new();

    internal IReadOnlyList<MainTabRegistration> Registrations => mainTabs;

    /// <summary>ShellViewModel이 생성되면서 실제 탭 전환 로직을 여기 꽂아준다.</summary>
    internal Action<string>? ActivateHandler { get; set; }

    public TabManager()
    {
        current = this;
    }

    /// <summary>
    /// key에 해당하는 메인탭으로 화면을 전환한다. Shell이 아직 안 떴거나 그런 key가
    /// 없으면 조용히 무시한다 — 탭 전환은 부가 기능이라 실패해도 호출부를 막을 정도는 아니다.
    /// </summary>
    public static void Activate(string key) => current?.ActivateHandler?.Invoke(key);

    /// <summary>
    /// 플랫폼 전용: 고정 "설정" 메인탭을 다른 등록보다 먼저 만들어둬서,
    /// RegisterViews/RegisterSettings에서 AddSub(SettingsTabKey, ...)로 서브탭을 바로 추가할 수 있게 한다.
    /// Order를 최대값으로 고정해 등록 순서와 무관하게 항상 마지막 탭으로 표시된다.
    /// </summary>
    internal void AddFixedMain<TView, TViewModel>(Icon icon, string key, string title)
        where TView : FrameworkElement, new()
        where TViewModel : class
    {
        mainTabs.Add(new MainTabRegistration(key, title, icon, CreateContentFactory<TView, TViewModel>(), int.MaxValue));
    }

    public void AddMain<TView, TViewModel>(Icon icon, string key, string? title = null)
        where TView : FrameworkElement, new()
        where TViewModel : class
    {
        if (mainTabs.Any(m => m.Key == key))
        {
            throw new InvalidOperationException($"'{key}' 메인탭은 이미 등록되어 있습니다.");
        }

        mainTabs.Add(new MainTabRegistration(key, title ?? key, icon, CreateContentFactory<TView, TViewModel>(), mainTabs.Count));
    }

    public void AddSub<TView, TViewModel>(string parentKey, string? title = null)
        where TView : FrameworkElement, new()
        where TViewModel : class
    {
        var parent = mainTabs.FirstOrDefault(m => m.Key == parentKey)
            ?? throw new InvalidOperationException($"'{parentKey}' 메인탭을 AddMain으로 먼저 등록해야 합니다.");

        parent.SubTabs.Add(new SubTabRegistration(title ?? typeof(TView).Name, CreateContentFactory<TView, TViewModel>(), parent.SubTabs.Count));
    }

    public void AddSettingsSub<TSettings>(string parentKey, string? title = null)
        where TSettings : class, new()
    {
        var parent = mainTabs.FirstOrDefault(m => m.Key == parentKey)
            ?? throw new InvalidOperationException($"'{parentKey}' 메인탭을 AddMain으로 먼저 등록해야 합니다.");

        var label = title ?? typeof(TSettings).Name;
        parent.SubTabs.Add(new SubTabRegistration(label, CreateSettingsContentFactory<TSettings>(), parent.SubTabs.Count));
    }

    private static Func<IServiceProvider, object> CreateSettingsContentFactory<TSettings>()
        where TSettings : class, new()
    {
        // JSON 저장 파일명은 화면에 보이는 제목(title)이 아니라 항상 클래스 이름으로 고정한다
        // (Settings/GeneralSettings.json 처럼) — 같은 클래스는 어느 탭에 붙여도 같은 파일을 공유한다.
        var key = typeof(TSettings).Name;

        return serviceProvider =>
        {
            var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            var instance = settingsService.RegisterSettings(key, new TSettings());
            var viewModel = new AutoSettingsViewModel(
                instance,
                () => settingsService.Save(key),
                () => settingsService.RegisterSettings(key, new TSettings()));
            return new AutoSettingsView { DataContext = viewModel };
        };
    }

    private static Func<IServiceProvider, object> CreateContentFactory<TView, TViewModel>()
        where TView : FrameworkElement, new()
        where TViewModel : class
    {
        return serviceProvider =>
        {
            var view = new TView
            {
                DataContext = ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider)
            };
            return view;
        };
    }
}
