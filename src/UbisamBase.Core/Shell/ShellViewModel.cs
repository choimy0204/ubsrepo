using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using UbisamBase.Core.Modules;
using UbisamBase.Core.Settings;
using UbisamBase.Core.Setup;

namespace UbisamBase.Core.Shell;

public partial class ShellViewModel : ObservableObject
{
    /// <summary>좌상단에 표기되는 회사명. 장비명(Title)과 구분선으로 나란히 놓인다.</summary>
    public string Brand { get; } = "UBISAM";

    /// <summary>좌상단 장비명. IAppSetup.GetAppName()이 돌려주는 값.</summary>
    public string Title { get; }

    public UiSettingsService UiSettings { get; }

    public ObservableCollection<MainTabViewModel> MainTabs { get; } = new();

    [ObservableProperty]
    private MainTabViewModel? selectedMainTab;

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string currentDate = FormatDate(DateTime.Now);

    private readonly DispatcherTimer clockTimer;

    public ShellViewModel(IServiceProvider serviceProvider, IAppSetup appSetup, TabManager tabManager, UiSettingsService uiSettings)
    {
        Title = appSetup.GetAppName();
        UiSettings = uiSettings;

        foreach (var registration in tabManager.Registrations.OrderBy(m => m.Order))
        {
            var mainTab = new MainTabViewModel(
                registration.Key,
                registration.Title,
                registration.Icon,
                () => registration.ContentFactory(serviceProvider),
                registration.Order);

            foreach (var sub in registration.SubTabs.OrderBy(s => s.Order))
            {
                mainTab.AddSubTab(new SubTabViewModel(sub.Title, () => sub.ContentFactory(serviceProvider), sub.Order));
            }

            MainTabs.Add(mainTab);
        }

        SelectedMainTab = MainTabs.FirstOrDefault();
        tabManager.ActivateHandler = ActivateMainTab;

        clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clockTimer.Tick += (_, _) =>
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = FormatDate(now);
        };
        clockTimer.Start();
    }

    /// <summary>TabManager.Activate(key)가 호출하는 실제 전환 로직. key가 없으면 무시한다.</summary>
    private void ActivateMainTab(string key)
    {
        var tab = MainTabs.FirstOrDefault(t => t.Key == key);
        if (tab is not null) SelectedMainTab = tab;
    }

    // 서브탭이 있는 메인 탭을 선택하면(예: 설정 탭) 맨 앞 서브탭이 자동으로 열리게 한다.
    // 이미 서브탭을 골라둔 적이 있으면(재방문) 그 선택을 유지한다.
    partial void OnSelectedMainTabChanged(MainTabViewModel? value)
    {
        if (value is { HasSubTabs: true, SelectedSubTab: null })
        {
            value.SelectedSubTab = value.SubTabs.FirstOrDefault();
        }
    }

    private static string FormatDate(DateTime value)
    {
        var day = value.DayOfWeek switch
        {
            DayOfWeek.Sunday => "일",
            DayOfWeek.Monday => "월",
            DayOfWeek.Tuesday => "화",
            DayOfWeek.Wednesday => "수",
            DayOfWeek.Thursday => "목",
            DayOfWeek.Friday => "금",
            _ => "토"
        };

        return $"{value:yyyy.MM.dd} {day}";
    }
}
