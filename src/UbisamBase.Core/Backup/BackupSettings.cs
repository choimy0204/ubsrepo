using System;
using System.Collections.Generic;
using System.ComponentModel;
using UbisamBase.Core.SettingItems;

namespace UbisamBase.Core.Backup;

public sealed class BackupSettings : ISettingItem
{
    // 전용 백업 화면에서 편집하므로 범용 설정 그리드에는 노출하지 않는다.
    [Browsable(false)]
    public TimeSpan BackupTime { get; set; } = new TimeSpan(2, 0, 0);

    [Browsable(false)]
    public List<BackupItem> Items { get; set; } = new();
}
