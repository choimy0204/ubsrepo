using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.SettingItems;

/// <summary>
/// 설정 POCO의 프로퍼티 하나를 리플렉션으로 감싸 편집 가능한 형태로 노출한다.
/// 프로퍼티 타입만으로 편집 UI 종류(<see cref="AutoSettingsFieldKind"/>)가 정해진다 —
/// bool/int/float,double/DateTime/enum(라디오)/string/ObservableCollection&lt;T&gt;.
/// DisplayName/Category/ReadOnly/Browsable 같은 표준 System.ComponentModel 속성을 인식한다 —
/// 예: [Category("계측"), DisplayName("측정값")] 처럼 프로퍼티 위에 붙이면 "계측" 말머리로
/// 묶이고 화면에는 "측정값"이라는 이름으로 표시된다. DisplayName을 생략하면 프로퍼티 이름 그대로 나온다.
/// </summary>
public partial class AutoSettingsField : ObservableObject
{
    private readonly object target;
    private readonly PropertyInfo property;

    public string Label { get; }
    public string? Category { get; }
    public bool IsEditable { get; }
    public AutoSettingsFieldKind Kind { get; }

    public bool IsBool => Kind == AutoSettingsFieldKind.Bool;
    public bool IsInt => Kind == AutoSettingsFieldKind.Int;
    public bool IsDecimal => Kind == AutoSettingsFieldKind.Decimal;
    public bool IsDateTime => Kind == AutoSettingsFieldKind.DateTime;
    public bool IsEnum => Kind == AutoSettingsFieldKind.Enum;
    public bool IsText => Kind == AutoSettingsFieldKind.Text;
    public bool IsCollection => Kind == AutoSettingsFieldKind.Collection;

    /// <summary>Enum 필드일 때 라디오 버튼으로 보여줄 선택지. 다른 종류면 비어 있다.</summary>
    public IReadOnlyList<EnumOption> EnumOptions { get; }

    public AutoSettingsField(object target, PropertyInfo property)
    {
        this.target = target;
        this.property = property;

        Label = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;
        Category = property.GetCustomAttribute<CategoryAttribute>()?.Category;
        var readOnlyAttr = property.GetCustomAttribute<ReadOnlyAttribute>();
        IsEditable = property.CanWrite && (readOnlyAttr == null || !readOnlyAttr.IsReadOnly);

        Kind = ResolveKind(property.PropertyType);
        EnumOptions = Kind == AutoSettingsFieldKind.Enum
            ? EnumOption.CreateFor(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType, () => Value, v => Value = v)
            : Array.Empty<EnumOption>();
    }

    private static AutoSettingsFieldKind ResolveKind(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(bool))
        {
            return AutoSettingsFieldKind.Bool;
        }

        if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short))
        {
            return AutoSettingsFieldKind.Int;
        }

        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
        {
            return AutoSettingsFieldKind.Decimal;
        }

        if (underlying == typeof(DateTime))
        {
            return AutoSettingsFieldKind.DateTime;
        }

        // [Flags] enum은 여러 값을 동시에 가질 수 있어 라디오(하나만 선택)로는 표현할 수 없다 — 텍스트("A, B")로 둔다.
        if (underlying.IsEnum && !underlying.IsDefined(typeof(FlagsAttribute), false))
        {
            return AutoSettingsFieldKind.Enum;
        }

        if (underlying.IsGenericType && underlying.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ObservableCollection<>))
        {
            return AutoSettingsFieldKind.Collection;
        }

        return AutoSettingsFieldKind.Text;
    }

    public object? Value
    {
        get => property.GetValue(target);
        set
        {
            if (!IsEditable || Kind == AutoSettingsFieldKind.Collection)
            {
                return;
            }

            try
            {
                property.SetValue(target, ConvertValue(value, property.PropertyType));
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

    private static object? ConvertValue(object? value, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value == null)
        {
            return underlying.IsValueType ? Activator.CreateInstance(underlying) : null;
        }

        if (underlying == typeof(string))
        {
            return value.ToString();
        }

        if (value.GetType() == underlying)
        {
            return value;
        }

        // Convert.ChangeType은 enum을 다루지 못한다. [Flags] enum을 텍스트("A, B")로 고친 경우도 여기로 온다.
        if (underlying.IsEnum)
        {
            return value is string text
                ? Enum.Parse(underlying, text, ignoreCase: true)
                : Enum.ToObject(underlying, value);
        }

        return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
    }
}
