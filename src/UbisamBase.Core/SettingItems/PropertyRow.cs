using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.SettingItems;

public enum PropertyEditorKind
{
    Bool,
    Enum,
    Text
}

/// <summary>
/// 설정 POCO의 프로퍼티 하나를 리플렉션으로 감싸서 편집 가능한 형태로 노출한다.
/// DisplayName/ReadOnly/Browsable 같은 표준 System.ComponentModel 속성을 인식한다.
/// </summary>
public partial class PropertyRow : ObservableObject
{
    private readonly object target;
    private readonly PropertyInfo property;

    public string Label { get; }
    public bool IsEditable { get; }
    public PropertyEditorKind Kind { get; }

    /// <summary>Enum 프로퍼티일 때 라디오 버튼으로 보여줄 선택지. 다른 종류면 비어 있다.</summary>
    public IReadOnlyList<EnumOption> EnumOptions { get; }

    public bool IsBool => Kind == PropertyEditorKind.Bool;
    public bool IsEnum => Kind == PropertyEditorKind.Enum;
    public bool IsText => Kind == PropertyEditorKind.Text;

    public PropertyRow(object target, PropertyInfo property)
    {
        this.target = target;
        this.property = property;

        Label = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;
        var readOnlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
        IsEditable = property.CanWrite && (readOnlyAttr == null || !readOnlyAttr.IsReadOnly);

        var type = property.PropertyType;
        if (type == typeof(bool))
        {
            Kind = PropertyEditorKind.Bool;
        }
        // [Flags] enum은 여러 값을 동시에 가질 수 있어 라디오(하나만 선택)로는 표현할 수 없다 — 텍스트("A, B")로 둔다.
        else if (type.IsEnum && !type.IsDefined(typeof(FlagsAttribute), false))
        {
            Kind = PropertyEditorKind.Enum;
        }
        else
        {
            Kind = PropertyEditorKind.Text;
        }

        EnumOptions = Kind == PropertyEditorKind.Enum
            ? EnumOption.CreateFor(type, () => Value, v => Value = v)
            : Array.Empty<EnumOption>();
    }

    public object? Value
    {
        get => property.GetValue(target);
        set
        {
            if (!IsEditable)
            {
                return;
            }

            try
            {
                var converted = Kind == PropertyEditorKind.Text ? ConvertText(value, property.PropertyType) : value;
                property.SetValue(target, converted);
            }
            catch
            {
                // 타입 변환 실패(잘못된 입력)는 조용히 무시하고 이전 값을 유지한다.
            }

            OnPropertyChanged();
            foreach (var option in EnumOptions)
            {
                option.NotifySelectionChanged();
            }
        }
    }

    private static object? ConvertText(object? value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        // Convert.ChangeType은 enum을 다루지 못한다 — [Flags] enum을 "A, B" 텍스트로 고친 경우.
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value?.ToString() ?? "", ignoreCase: true);
        }

        return Convert.ChangeType(value, targetType);
    }
}
