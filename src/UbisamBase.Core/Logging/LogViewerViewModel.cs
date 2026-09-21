using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Logging;

public partial class LogViewerViewModel : ObservableObject
{
    public ObservableCollection<LogEntry> Entries => LogService.Current.RecentEntries;

    /// <summary>DataGrid가 실제로 바인딩하는 건 이 필터링된 뷰다 — 새 로그가 Entries에 들어와도
    /// 그대로 반영되면서, SearchText/SelectedLevelOption에 맞지 않는 항목은 걸러서 보여준다.
    /// 기록 자체(LogService.RecentEntries/파일)는 항상 전부 남는다 — 이건 순수 화면 필터다.</summary>
    public ICollectionView EntriesView { get; }

    [ObservableProperty]
    private string searchText = string.Empty;

    public IReadOnlyList<LogLevelFilterOption> LevelOptions { get; } = new[]
    {
        new LogLevelFilterOption(null, "ALL"),
        new LogLevelFilterOption(LogLevel.Verbose, "Verbose"),
        new LogLevelFilterOption(LogLevel.Debug, "Debug"),
        new LogLevelFilterOption(LogLevel.Info, "Info"),
        new LogLevelFilterOption(LogLevel.Warning, "Warning"),
        new LogLevelFilterOption(LogLevel.Error, "Error"),
    };

    /// <summary>ALL이면 전부 통과, 그 외엔 딱 그 레벨과 정확히 같은 항목만 통과시킨다 —
    /// "이상"이 아니라 "정확히 이 레벨"만 찾는다.</summary>
    [ObservableProperty]
    private LogLevelFilterOption selectedLevelOption;

    public LogViewerViewModel()
    {
        selectedLevelOption = LevelOptions[0];

        // GetDefaultView(Entries)는 컬렉션당 하나로 공유되는 뷰라, 이 화면을 두 곳(설정 탭 + 로그 팝업)에서
        // 동시에 띄우면 필터가 서로 간섭한다. new CollectionViewSource로 인스턴스마다 독립된 뷰를 만든다.
        EntriesView = new CollectionViewSource { Source = Entries }.View;
        EntriesView.Filter = FilterEntry;
    }

    private bool FilterEntry(object obj)
    {
        if (obj is not LogEntry entry)
        {
            return false;
        }

        if (SelectedLevelOption?.Level is { } level && entry.Level != level)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return Contains(entry.Message) || Contains(entry.Tag) || Contains(entry.Level.ToString());

        bool Contains(string value) => value.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    partial void OnSearchTextChanged(string value) => EntriesView.Refresh();

    partial void OnSelectedLevelOptionChanged(LogLevelFilterOption value) => EntriesView.Refresh();
}
