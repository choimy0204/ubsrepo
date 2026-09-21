using System.Collections.Generic;
using System.ComponentModel;
using UbisamBase.Core.SettingItems;

namespace UbisamBase.Core.Cleaning;

public sealed class CleanerSettings : ISettingItem
{
    // 전용 클리너 화면에서 편집하므로 범용 설정 그리드(AllSettingsView)에는 노출하지 않는다.
    [Browsable(false)]
    public List<CleanerRule> Rules { get; set; } = new();
}
