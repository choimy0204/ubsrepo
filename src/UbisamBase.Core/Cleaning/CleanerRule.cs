using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Cleaning;

/// <summary>
/// 클리너 규칙 하나 — 폴더 안의 오래된 파일(확장자 기준) 또는 이름에 날짜가 포함된 하위 폴더를
/// 기한이 지나면 자동으로 지운다. Enabled(사용 여부)와 Applied(적용 여부) 둘 다 켜져 있어야
/// 실제로 삭제가 실행된다 — 경로/확장자를 고쳐놓고 "적용"을 깜빡한 채 방치되는 걸 막는 안전장치다.
/// </summary>
public sealed partial class CleanerRule : ObservableObject
{
    [ObservableProperty]
    private bool enabled = true;

    [ObservableProperty]
    private string path = "";

    [ObservableProperty]
    private string extension = "";

    [ObservableProperty]
    private int days = 30;

    [ObservableProperty]
    private bool isFolderMode;

    [ObservableProperty]
    private bool applied;

    [ObservableProperty]
    private string lastDeleted = "";

    public string StatusText => Applied ? "적용됨" : "미적용";

    partial void OnAppliedChanged(bool value) => OnPropertyChanged(nameof(StatusText));
}
