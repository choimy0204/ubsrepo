using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Settings;

public partial class SettingsHomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "하단의 UI Setting 탭에서 테마, 네비게이션 위치, 화면 배율을 바꿀 수 있습니다.";
}
