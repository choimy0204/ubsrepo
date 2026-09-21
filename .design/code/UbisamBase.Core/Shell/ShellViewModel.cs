using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using UbisamBase.Core.Auth;
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

    public AuthService Auth { get; }

    public ObservableCollection<MainTabViewModel> MainTabs { get; } = new();

    [ObservableProperty]
    private MainTabViewModel? selectedMainTab;

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string currentDate = FormatDate(DateTime.Now);

    private readonly DispatcherTimer clockTimer;

    public ShellViewModel(IServiceProvider serviceProvider, IAppSetup appSetup, TabManager tabManager, UiSettingsService uiSettings, AuthService auth)
    {
        Title = appSetup.GetAppName();
        UiSettings = uiSettings;
        Auth = auth;

        foreach (var registration in tabManager.Registrations.OrderBy(m => m.Order))
        {
            var mainTab = new MainTabViewModel(
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

        clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clockTimer.Tick += (_, _) =>
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = FormatDate(now);
        };
        clockTimer.Start();
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
