using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace UbisamBase.Core.Locking;

/// <summary>
/// "화면 잠금" 버튼을 누르면 뜨는 확인 창. 저장된 계정을 검증하는 게 아니라, 여기서 입력한
/// 아이디/비밀번호가 그대로 이번 잠금의 해제 키가 된다 — LOCK을 누르면 그 값으로 잠긴다.
/// </summary>
public partial class LockConfirmDialog : Window
{
    private static readonly string LastLockFilePath =
        Path.Combine(@"D:\UbisamConfig\ScreenSaver", "last-lock.json");

    public LockConfirmDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => IdBox.Focus();

        if (Application.Current?.MainWindow is { IsLoaded: true } main && !ReferenceEquals(main, this))
        {
            Owner = main;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void Lock_Click(object sender, RoutedEventArgs e)
    {
        var id = IdBox.Text;
        var password = PasswordBoxInput.Password;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(password))
        {
            ErrorText.Text = "아이디와 비밀번호를 입력하세요.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        SaveLastLock(id, password);

        // 지금 입력한 값이 이번 잠금의 해제 키가 된다.
        LockService.Current.Validator = (enteredId, enteredPassword) => enteredId == id && enteredPassword == password;
        LockService.Current.Lock();
        Close();
    }

    /// <summary>요청에 따라 마지막으로 잠글 때 쓴 아이디/비밀번호를 평문 그대로 남긴다.
    /// 이 파일이 유출되면 비밀번호가 그대로 노출된다는 걸 알고 있는 상태에서
    /// 명시적으로 요청받은 동작이다.</summary>
    private static void SaveLastLock(string id, string password)
    {
        try
        {
            var dir = Path.GetDirectoryName(LastLockFilePath)!;
            Directory.CreateDirectory(dir);

            var record = new { Id = id, Password = password, LockedAt = DateTimeOffset.Now };
            File.WriteAllText(LastLockFilePath, JsonSerializer.Serialize(record));
        }
        catch
        {
            // 기록 실패는 잠금 자체를 막지 않는다.
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    public static void ShowAndLock() => new LockConfirmDialog().ShowDialog();
}
