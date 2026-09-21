using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using UbisamBase.Core.Logging;
using UbisamBase.Core.SettingItems;

namespace UbisamBase.Core.Cleaning;

/// <summary>
/// 매분 날짜가 바뀌었는지 확인해서, 하루에 한 번(자정 지나 첫 확인 시점) 활성화·적용된 규칙을
/// 전부 실행하는 자동 삭제 서비스. 사내 Aurora 원격 제어 프로그램의 "자동 삭제" 탭 로직을
/// 그대로 이식한 것 — 파일 모드는 최종 수정 시각 기준, 폴더 모드는 폴더 이름에 포함된 날짜
/// (yyyyMMdd 또는 yyMMdd) 기준으로 기한을 판정한다. 수동 즉시 실행(RunNow)도 지원한다.
/// </summary>
public sealed class CleanerService
{
    private const string SettingsKey = "__Cleaner";
    private static readonly Logger Log = new("Cleaner");

    private readonly ISettingsService settingsService;
    private readonly DispatcherTimer checkTimer;
    private DateTime lastRunDate = DateTime.MinValue;

    public CleanerSettings Settings { get; }

    public CleanerService(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
        Settings = settingsService.Register(SettingsKey, new CleanerSettings());

        checkTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        checkTimer.Tick += (_, _) => CheckSchedule();
        checkTimer.Start();
    }

    private void CheckSchedule()
    {
        var today = DateTime.Now.Date;
        if (today == lastRunDate)
        {
            return;
        }

        lastRunDate = today;
        RunNow();
    }

    /// <summary>사용(Enabled)+적용(Applied) 둘 다 켜진 규칙만 실행한다. 실제로 지운 파일·폴더
    /// 총 개수를 돌려준다 — 호출부(UI)가 "몇 개 지웠는지"를 사용자에게 바로 알려줄 수 있게.</summary>
    public int RunNow()
    {
        var totalDeleted = 0;
        foreach (var rule in Settings.Rules.Where(r => r.Enabled && r.Applied))
        {
            totalDeleted += ApplyRule(rule);
        }

        Save();
        return totalDeleted;
    }

    public void Save() => settingsService.Save(SettingsKey);

    /// <summary>디스크에 저장된 값으로 Settings를 다시 채운다(참조는 그대로 유지된다).</summary>
    public void Reload() => settingsService.Reload(SettingsKey);

    private static int ApplyRule(CleanerRule rule)
    {
        var folder = rule.Path?.Trim() ?? "";
        if (string.IsNullOrEmpty(folder) || rule.Days <= 0)
        {
            return 0;
        }

        if (!Directory.Exists(folder))
        {
            Log.W($"클리너: 폴더 없음 — {folder}");
            return 0;
        }

        var now = DateTime.Now;
        var threshold = TimeSpan.FromDays(rule.Days);
        var deleted = 0;

        if (rule.IsFolderMode)
        {
            deleted = DeleteOldFolders(folder, threshold, now);
        }
        else
        {
            var ext = rule.Extension?.Trim().TrimStart('.') ?? "";
            if (string.IsNullOrEmpty(ext))
            {
                return 0;
            }

            deleted = DeleteOldFiles(folder, ext, threshold, now);
        }

        if (deleted > 0)
        {
            rule.LastDeleted = now.ToString("yyyy-MM-dd HH:mm");
        }

        return deleted;
    }

    /// <summary>지금 실행 중인 프로그램 자신의 폴더는 지우지 않는다(사용 중 보호).</summary>
    private static bool IsSelfProtected(string candidateDir)
    {
        var selfDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
        var normalized = Path.GetFullPath(candidateDir).TrimEnd('\\', '/');
        return selfDir.StartsWith(normalized, StringComparison.OrdinalIgnoreCase);
    }

    private static int DeleteOldFolders(string folder, TimeSpan threshold, DateTime now)
    {
        var deleted = 0;
        foreach (var dir in Directory.GetDirectories(folder))
        {
            if (IsSelfProtected(dir))
            {
                Log.I($"클리너: 스킵(사용 중) — {dir}");
                continue;
            }

            var folderDate = ParseFolderDate(Path.GetFileName(dir));
            if (folderDate is null || now - folderDate.Value < threshold)
            {
                continue;
            }

            try
            {
                Directory.Delete(dir, recursive: true);
                deleted++;
                Log.I($"클리너: 폴더 삭제 — {dir} ({(now - folderDate.Value).Days}일 경과)");
            }
            catch (Exception ex)
            {
                Log.E($"클리너: 폴더 삭제 실패 — {dir}", ex);
            }
        }

        return deleted;
    }

    private static int DeleteOldFiles(string folder, string ext, TimeSpan threshold, DateTime now)
    {
        var deleted = 0;
        foreach (var file in Directory.GetFiles(folder, $"*.{ext}"))
        {
            var age = now - File.GetLastWriteTime(file);
            if (age < threshold)
            {
                continue;
            }

            try
            {
                File.Delete(file);
                deleted++;
                Log.I($"클리너: 파일 삭제 — {file} (최종수정 {age.Days}일 전)");
            }
            catch (Exception ex)
            {
                Log.E($"클리너: 파일 삭제 실패 — {file}", ex);
            }
        }

        return deleted;
    }

    /// <summary>폴더 이름에서 날짜를 파싱한다. yyyyMMdd(8자리) → yyMMdd(6자리) 순으로 시도, 실패하면 null.</summary>
    private static DateTime? ParseFolderDate(string name)
    {
        var m8 = Regex.Match(name, @"\d{8}");
        if (m8.Success && DateTime.TryParseExact(m8.Value, "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d8))
        {
            return d8;
        }

        var m6 = Regex.Match(name, @"\d{6}");
        if (m6.Success && DateTime.TryParseExact(m6.Value, "yyMMdd",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d6))
        {
            return d6;
        }

        return null;
    }
}
