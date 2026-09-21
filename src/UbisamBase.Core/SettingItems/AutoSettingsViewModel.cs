using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.SettingItems;

/// <summary>
/// View 없이 설정 POCO의 public 프로퍼티만으로 편집 화면을 구성하는 뷰모델.
/// ITabManager.AddSettingsSub로 등록하면 Shell이 이 뷰모델 + <see cref="AutoSettingsView"/>를
/// 대상 인스턴스에 연결해 서브탭으로 보여준다. [Category("말머리")]가 같은 프로퍼티끼리 묶여
/// <see cref="Groups"/>로 노출된다.
/// </summary>
public partial class AutoSettingsViewModel : ObservableObject
{
    private object settingsInstance;
    private readonly Action save;
    private readonly Func<object> reload;

    public ObservableCollection<AutoSettingsFieldGroup> Groups { get; } = new();

    public AutoSettingsViewModel(object settingsInstance, Action save, Func<object> reload)
    {
        this.settingsInstance = settingsInstance;
        this.save = save;
        this.reload = reload;
        BuildGroups();
    }

    private void BuildGroups()
    {
        Groups.Clear();

        foreach (var prop in settingsInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead)
            {
                continue;
            }

            var browsable = prop.GetCustomAttribute<BrowsableAttribute>();
            if (browsable is { Browsable: false })
            {
                continue;
            }

            var field = new AutoSettingsField(settingsInstance, prop);

            var group = Groups.FirstOrDefault(g => g.Category == field.Category);
            if (group == null)
            {
                group = new AutoSettingsFieldGroup(field.Category);
                Groups.Add(group);
            }

            group.Fields.Add(field);
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            save();
            MessageUtil.ShowSavedToast();
        }
        catch
        {
            MessageUtil.ShowErrorToast("저장하지 못했습니다.");
        }
    }

    /// <summary>디스크에 저장된 값으로 화면을 다시 불러온다 — 편집 중인 값을 버리고 되돌릴 때 쓴다.</summary>
    [RelayCommand]
    private void Refresh()
    {
        try
        {
            settingsInstance = reload();
            BuildGroups();
            MessageUtil.ShowSuccessToast("새로고침되었습니다.");
        }
        catch
        {
            MessageUtil.ShowErrorToast("새로고침하지 못했습니다.");
        }
    }
}
