using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Threading;
using UbisamBase.Core.Logging;
using UbisamBase.Core.SettingItems;

namespace UbisamBase.Core.Backup;

/// <summary>
/// 매분 스케줄을 확인해서 지정된 시각에 하루 한 번 자동 백업하고, 개수 기준으로 오래된 백업을 정리한다.
/// 수동 백업(RunBackup)도 지원한다.
/// </summary>
public sealed class BackupService
{
    private const string SettingsKey = "__Backup";
    private static readonly Logger Log = new("Backup");

    private readonly ISettingsService settingsService;
    private readonly DispatcherTimer checkTimer;
    private DateTime lastRunDate = DateTime.MinValue;

    public BackupSettings Settings { get; }

    public BackupService(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
        Settings = settingsService.Register(SettingsKey, new BackupSettings());

        checkTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        checkTimer.Tick += (_, _) => CheckSchedule();
        checkTimer.Start();
    }

    private void CheckSchedule()
    {
        var now = DateTime.Now;
        if (now.Date == lastRunDate || now.TimeOfDay < Settings.BackupTime)
        {
            return;
        }

        lastRunDate = now.Date;
        RunBackup();
    }

    public void RunBackup()
    {
        foreach (var item in Settings.Items.Where(i => i.Enabled))
        {
            BackupOne(item);
        }
    }

    public void Save() => settingsService.Save(SettingsKey);

    /// <summary>디스크에 저장된 값으로 Settings를 다시 채운다(참조는 그대로 유지된다).</summary>
    public void Reload() => settingsService.Reload(SettingsKey);

    private static void BackupOne(BackupItem item)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(item.SourcePath) || !Directory.Exists(item.SourcePath))
            {
                Log.W($"백업 소스 경로가 없습니다: {item.SourcePath}");
                return;
            }

            Directory.CreateDirectory(item.DestinationPath);
            var fileName = $"{item.Name}_{DateTime.Now:yyyyMMdd-HHmmss}.zip";
            var destFile = Path.Combine(item.DestinationPath, fileName);

            ZipFile.CreateFromDirectory(item.SourcePath, destFile);
            Log.I($"백업 완료: {destFile}");

            CleanupOldBackups(item);
        }
        catch (Exception ex)
        {
            Log.E($"백업 실패: {item.Name}", ex);
        }
    }

    private static void CleanupOldBackups(BackupItem item)
    {
        var files = Directory.GetFiles(item.DestinationPath, $"{item.Name}_*.zip")
            .OrderByDescending(f => f)
            .Skip(item.KeepCount);

        foreach (var old in files)
        {
            try
            {
                File.Delete(old);
            }
            catch
            {
                // 삭제 실패는 다음 정리 때 다시 시도된다.
            }
        }
    }
}
