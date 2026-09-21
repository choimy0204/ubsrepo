using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.SettingItems;

/// <summary>
/// ISettingsService에 등록된 모든 설정 항목을 좌측 목록으로 보여주고,
/// 선택한 항목을 우측 PropertyGrid로 편집/저장하는 플랫폼 고정 화면.
/// </summary>
public partial class AllSettingsViewModel : ObservableObject
{
    private readonly ISettingsService settingsService;

    public ObservableCollection<string> Keys { get; } = new();

    public SettingsPropertyGridViewModel Grid { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private string? selectedKey;

    public bool HasSelection => SelectedKey != null;

    public AllSettingsViewModel(ISettingsService settingsService)
    {
        this.settingsService = settingsService;

        foreach (var key in settingsService.RegisteredKeys)
        {
            Keys.Add(key);
        }

        SelectedKey = Keys.FirstOrDefault();
    }

    partial void OnSelectedKeyChanged(string? value)
    {
        Grid.Load(value != null ? settingsService.GetRaw(value) : null);
    }

    [RelayCommand]
    private void Save()
    {
        if (SelectedKey == null)
        {
            return;
        }

        try
        {
            settingsService.Save(SelectedKey);
            MessageUtil.ShowSavedToast();
        }
        catch
        {
            MessageUtil.ShowErrorToast("저장하지 못했습니다.");
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        if (SelectedKey == null)
        {
            return;
        }

        try
        {
            settingsService.Reload(SelectedKey);
            Grid.Load(settingsService.GetRaw(SelectedKey));
            MessageUtil.ShowSuccessToast("새로고침되었습니다.");
        }
        catch
        {
            MessageUtil.ShowErrorToast("새로고침하지 못했습니다.");
        }
    }
}
