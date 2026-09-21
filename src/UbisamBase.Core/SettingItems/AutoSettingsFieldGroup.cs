using System.Collections.ObjectModel;

namespace UbisamBase.Core.SettingItems;

/// <summary>
/// [Category("이름")]가 같은 AutoSettingsField들을 한데 묶은 것. Category가 없는 프로퍼티는
/// Category가 null인 그룹 하나에 모이고, 화면에서는 이 그룹만 말머리 없이 표시된다.
/// </summary>
public sealed class AutoSettingsFieldGroup
{
    public string? Category { get; }
    public bool HasCategory => Category != null;
    public ObservableCollection<AutoSettingsField> Fields { get; } = new();

    public AutoSettingsFieldGroup(string? category)
    {
        Category = category;
    }
}
