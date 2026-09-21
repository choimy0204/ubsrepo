using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace UbisamBase.Core.SettingItems;

public enum NumericInputMode
{
    None,
    Integer,
    Decimal
}

/// <summary>
/// TextBox에 붙이면 Mode에 맞지 않는 문자 입력/붙여넣기를 막는다.
/// AutoSettingsView의 int/float,double 필드가 "숫자만 입력 가능한 텍스트박스"가 되도록 쓴다.
/// </summary>
public static class NumericInputBehavior
{
    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.RegisterAttached(
            "Mode",
            typeof(NumericInputMode),
            typeof(NumericInputBehavior),
            new PropertyMetadata(NumericInputMode.None, OnModeChanged));

    public static void SetMode(DependencyObject element, NumericInputMode value) => element.SetValue(ModeProperty, value);

    public static NumericInputMode GetMode(DependencyObject element) => (NumericInputMode)element.GetValue(ModeProperty);

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.PreviewTextInput -= OnPreviewTextInput;
        DataObject.RemovePastingHandler(textBox, OnPaste);

        if ((NumericInputMode)e.NewValue != NumericInputMode.None)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(textBox, OnPaste);
        }
    }

    private static void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        var textBox = (TextBox)sender;
        e.Handled = !IsValid(GetProposedText(textBox, e.Text), GetMode(textBox));
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        var textBox = (TextBox)sender;
        var pasted = (string)e.DataObject.GetData(typeof(string));
        if (!IsValid(GetProposedText(textBox, pasted), GetMode(textBox)))
        {
            e.CancelCommand();
        }
    }

    private static string GetProposedText(TextBox textBox, string input)
    {
        var text = textBox.Text;
        return text.Substring(0, textBox.SelectionStart)
               + input
               + text.Substring(textBox.SelectionStart + textBox.SelectionLength);
    }

    private static bool IsValid(string proposed, NumericInputMode mode)
    {
        if (proposed.Length == 0)
        {
            return true;
        }

        var pattern = mode == NumericInputMode.Integer ? @"^-?\d*$" : @"^-?\d*\.?\d*$";
        return Regex.IsMatch(proposed, pattern);
    }
}
