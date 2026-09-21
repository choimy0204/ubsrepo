namespace UbisamBase.Core.Modules;

/// <summary>
/// AddMain에 넘길 수 있는 아이콘 목록. Themes/Icons.xaml에 등록된 Geometry 리소스와 이름이 1:1로 대응한다
/// (예: Icon.Home → "Ubisam.Icon.Home"). 아이콘 없이 라벨만 쓰려면 Icon.None을 넘긴다.
/// </summary>
public enum Icon
{
    None,
    Home,
    Monitor,
    Chart,
    Maintenance,
    Settings,
    Alarm,
    Log,
    Document,
    Data,
    Power,
    Temperature,
    Gauge,
    Run,
    Stop,
    Reset,
    Account,
    Lock,
    Export,
    Import,
    Search,
    Calendar,
    Clock,
    Link,
    Warning,
    Pass,
    Folder,
    Vision,
    Control,
    Recipe,
    Modules,
    Save,
    Desktop,
    Camera,
    Fullscreen,
    FullscreenExit
}
