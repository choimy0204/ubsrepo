using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.SettingItems;

/// <summary>
/// 설정 화면에서 enum 값을 라디오 버튼으로 고를 때의 선택지 하나. 선택되면 원래 값을 이 값으로 바꾼다.
/// 표시 이름은 enum 멤버의 [Description]을 쓰고, 없으면 멤버 이름 그대로 쓴다 —
/// 예: <c>[Description("자동")] Auto</c>.
/// </summary>
public sealed class EnumOption : ObservableObject
{
    private readonly Func<object?> getValue;
    private readonly Action<object> setValue;

    public string Label { get; }

    public object Value { get; }

    /// <summary>
    /// 같은 값의 라디오끼리만 서로 배타적으로 묶이도록 값마다 고유한 이름을 쓴다.
    /// ItemsControl 안의 RadioButton은 각자 다른 부모에 들어가서, GroupName이 없으면 서로 묶이지 않는다.
    /// </summary>
    public string GroupName { get; }

    private EnumOption(Func<object?> getValue, Action<object> setValue, string groupName, string label, object value)
    {
        this.getValue = getValue;
        this.setValue = setValue;
        GroupName = groupName;
        Label = label;
        Value = value;
    }

    public bool IsSelected
    {
        get => Equals(getValue(), Value);
        set
        {
            // 다른 라디오가 선택되면서 이 라디오가 꺼지는 경우(false)는 무시한다 — 값은 새로 선택된 쪽이 바꾼다.
            if (value && !IsSelected)
            {
                setValue(Value);
            }
        }
    }

    internal void NotifySelectionChanged() => OnPropertyChanged(nameof(IsSelected));

    internal static IReadOnlyList<EnumOption> CreateFor(Type enumType, Func<object?> getValue, Action<object> setValue)
    {
        var groupName = "UbisamEnum_" + Guid.NewGuid().ToString("N");
        return enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(member => new EnumOption(
                getValue,
                setValue,
                groupName,
                member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? member.Name,
                member.GetValue(null)!))
            .ToList();
    }
}
