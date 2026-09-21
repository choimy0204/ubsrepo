using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.Cleaning;

public partial class CleanerViewModel : ObservableObject
{
    private readonly CleanerService service;
    private readonly DispatcherTimer countdownTimer;

    public ObservableCollection<CleanerRule> Rules { get; }

    /// <summary>규칙이 하나도 없을 때 빈 목록 대신 안내 문구를 보여주기 위한 플래그.
    /// ObservableCollection.Count 변화는 WPF가 자동으로 감지해 이 바인딩을 갱신한다.</summary>
    public bool HasNoRules => Rules.Count == 0;

    [ObservableProperty]
    private string nextRunText = "";

    public CleanerViewModel(CleanerService service)
    {
        this.service = service;
        Rules = new ObservableCollection<CleanerRule>(service.Settings.Rules);
        Rules.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoRules));

        UpdateNextRunText();
        countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        countdownTimer.Tick += (_, _) => UpdateNextRunText();
        countdownTimer.Start();
    }

    /// <summary>다음 자정(=다음 자동 실행 시점)까지 남은 시간을 "N시간 M분 후"로 표시한다.
    /// 정확한 초 단위 카운트다운이 아니라 30초마다 갱신되는 대략적인 안내다 — 이 정도로 충분하다.</summary>
    private void UpdateNextRunText()
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var remaining = nextMidnight - now;
        NextRunText = remaining.Hours > 0
            ? $"{remaining.Hours}시간 {remaining.Minutes}분 후 (자정)"
            : $"{remaining.Minutes}분 후 (자정)";
    }

    [RelayCommand]
    private void AddRule() => Rules.Add(new CleanerRule());

    [RelayCommand]
    private void RemoveRule(CleanerRule? rule)
    {
        if (rule != null)
        {
            Rules.Remove(rule);
            ApplyToService();
            service.Save();
        }
    }

    /// <summary>규칙 한 개를 검증하고 "적용됨"으로 켠다 — 경로/확장자를 고친 뒤 반드시 다시 눌러야
    /// 그 변경이 실제 삭제 실행에 반영된다(Enabled와 별개의 이중 안전장치).</summary>
    [RelayCommand]
    private void ApplyRule(CleanerRule? rule)
    {
        if (rule == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(rule.Path))
        {
            MessageUtil.ShowWarningToast("폴더 경로를 입력하세요.");
            return;
        }

        if (!rule.IsFolderMode && string.IsNullOrWhiteSpace(rule.Extension))
        {
            MessageUtil.ShowWarningToast("확장자를 입력하거나 폴더삭제를 체크하세요.");
            return;
        }

        rule.Applied = true;
        ApplyToService();
        service.Save();
    }

    /// <summary>경로/확장자/폴더삭제 여부를 편집하면 호출된다(CleanerView 코드비하인드) — 재검증 없이
    /// 예전 경로 기준으로 삭제가 계속되는 걸 막기 위해 적용 상태를 되돌린다.</summary>
    public void MarkUnapplied(CleanerRule rule)
    {
        if (rule.Applied)
        {
            rule.Applied = false;
            ApplyToService();
            service.Save();
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            ApplyToService();
            service.Save();
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
        try
        {
            service.Reload();
            Rules.Clear();
            foreach (var rule in service.Settings.Rules)
            {
                Rules.Add(rule);
            }
            MessageUtil.ShowSuccessToast("새로고침되었습니다.");
        }
        catch
        {
            MessageUtil.ShowErrorToast("새로고침하지 못했습니다.");
        }
    }

    [RelayCommand]
    private void RunNow()
    {
        // 화면에 보이는 목록 그대로 실행되도록, 실행 전에 서비스 쪽 목록도 동기화한다.
        ApplyToService();
        var deleted = service.RunNow();
        if (deleted > 0)
        {
            MessageUtil.ShowSuccessToast($"클리너 실행 완료 — {deleted}개 삭제됨");
        }
        else
        {
            MessageUtil.ShowToast("클리너를 실행했습니다 — 삭제 대상 없음");
        }
    }

    private void ApplyToService() => service.Settings.Rules = Rules.ToList();
}
