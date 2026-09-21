using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Logging;

/// <summary>
/// 파일 로깅 + 최근 로그 버퍼(로그 뷰어용)를 담당하는 싱글턴.
/// <see cref="Logger"/>가 DI 없이도 어디서나 로그를 남길 수 있도록 정적 인스턴스로 노출한다.
/// </summary>
public sealed partial class LogService : ObservableObject
{
    private const int MaxRecentEntries = 2000;

    public static LogService Current { get; } = new();

    [ObservableProperty]
    private LogLevel minLevel = LogLevel.Verbose;

    public ObservableCollection<LogEntry> RecentEntries { get; } = new();

    private readonly object fileLock = new();
    private string? logDirectory;

    private LogService()
    {
    }

    /// <summary>로그 저장 위치를 정한다. 플랫폼 공통 설정 위치(D:\UbisamConfig)의 Log 폴더에 쌓는다 —
    /// 여러 프로그램이 이 플랫폼으로 만들어져도 항상 같은 곳에 모인다.</summary>
    public void Initialize(string basePath)
    {
        logDirectory = Path.Combine(basePath, "Log");
        try
        {
            Directory.CreateDirectory(logDirectory);
        }
        catch
        {
            logDirectory = null;
        }
    }

    internal void Write(string tag, LogLevel level, string message)
    {
        if (level < MinLevel)
        {
            return;
        }

        var entry = new LogEntry(DateTime.Now, tag, level, message);
        WriteToFile(entry);

        var app = Application.Current;
        app?.Dispatcher.BeginInvoke(new Action(() =>
        {
            RecentEntries.Add(entry);
            while (RecentEntries.Count > MaxRecentEntries)
            {
                RecentEntries.RemoveAt(0);
            }
        }));
    }

    private void WriteToFile(LogEntry entry)
    {
        if (logDirectory == null)
        {
            return;
        }

        try
        {
            var path = Path.Combine(logDirectory, $"{DateTime.Now:yyyyMMdd}.log");
            lock (fileLock)
            {
                File.AppendAllText(path, entry + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch
        {
            // 로그 파일 쓰기 실패는 앱 동작을 막지 않는다.
        }
    }
}
