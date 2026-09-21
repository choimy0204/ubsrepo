using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UbisamBase.Core.Localization;
using UbisamBase.Core.Messaging;
using UbisamBase.Core.Update;

namespace UbisamBase.Core.Settings;

public sealed class ThemeOption
{
    public ThemeMode Mode { get; }
    public string Label { get; }

    public ThemeOption(ThemeMode mode, string label)
    {
        Mode = mode;
        Label = label;
    }
}

public sealed class NavPositionOption
{
    public NavPosition Position { get; }
    public string Label { get; }

    public NavPositionOption(NavPosition position, string label)
    {
        Position = position;
        Label = label;
    }
}

public sealed class ToastPositionOption
{
    public ToastPosition Position { get; }
    public string Label { get; }

    public ToastPositionOption(ToastPosition position, string label)
    {
        Position = position;
        Label = label;
    }
}

/// <summary>UI Setting 화면의 ViewModel. 실제 상태는 싱글턴 UiSettingsService에 위임한다.</summary>
public partial class UiSettingsViewModel : ObservableObject
{
    private readonly UiSettingsService settings;
    private readonly LanguageService language;

    public IReadOnlyList<ThemeOption> ThemeOptions { get; } = new[]
    {
        new ThemeOption(ThemeMode.System, "시스템 설정 따르기"),
        new ThemeOption(ThemeMode.Light, "라이트"),
        new ThemeOption(ThemeMode.Dark, "다크"),
    };

    public IReadOnlyList<NavPositionOption> NavPositionOptions { get; } = new[]
    {
        new NavPositionOption(NavPosition.Left, "왼쪽"),
        new NavPositionOption(NavPosition.Right, "오른쪽"),
        new NavPositionOption(NavPosition.Bottom, "하단"),
    };

    public IReadOnlyList<ToastPositionOption> ToastPositionOptions { get; } = new[]
    {
        new ToastPositionOption(ToastPosition.TopLeft, "좌상단"),
        new ToastPositionOption(ToastPosition.TopRight, "우상단"),
        new ToastPositionOption(ToastPosition.BottomLeft, "좌하단"),
        new ToastPositionOption(ToastPosition.BottomRight, "우하단"),
    };

    public UiSettingsViewModel(UiSettingsService settings, LanguageService language)
    {
        this.settings = settings;
        this.language = language;
    }

    public IReadOnlyList<string> AvailableCultures => language.AvailableCultures;

    public bool HasLanguages => AvailableCultures.Count > 0;

    public string CurrentCulture
    {
        get => language.CurrentCulture;
        set
        {
            language.CurrentCulture = value;
            OnPropertyChanged();
        }
    }

    public ThemeOption SelectedThemeOption
    {
        get => ThemeOptions.First(o => o.Mode == settings.ThemeMode);
        set
        {
            settings.ThemeMode = value.Mode;
            OnPropertyChanged();
        }
    }

    public NavPositionOption SelectedNavPositionOption
    {
        get => NavPositionOptions.First(o => o.Position == settings.NavPosition);
        set
        {
            settings.NavPosition = value.Position;
            OnPropertyChanged();
        }
    }

    public ToastPositionOption SelectedToastPositionOption
    {
        get => ToastPositionOptions.First(o => o.Position == settings.ToastPosition);
        set
        {
            settings.ToastPosition = value.Position;
            OnPropertyChanged();
        }
    }

    /// <summary>플랫폼 업데이트 설정(D:\UbisamPlatform\update-settings.json). UiSettingsService와는
    /// 다른 파일이지만 화면과 저장/새로고침 버튼은 같이 쓴다 — 사용자 입장에선 한 화면이다.</summary>
    private PlatformUpdateSettings Update => PlatformUpdateSettings.Current;

    /// <summary>실행할 때 깃 저장소와 비교해 새 버전을 확인할지.</summary>
    public bool UpdateEnabled
    {
        get => Update.Enabled;
        set
        {
            Update.Enabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>true면 묻지 않고 바로 받는다.</summary>
    public bool UpdateAuto
    {
        get => Update.AutoUpdate;
        set
        {
            Update.AutoUpdate = value;
            OnPropertyChanged();
        }
    }

    public string UpdateRepositoryUrl
    {
        get => Update.RepositoryUrl;
        set
        {
            Update.RepositoryUrl = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>지금 깔려 있는 플랫폼 버전(배포할 때 찍힌 시각). 보여주기만 한다.</summary>
    public string InstalledPlatformVersion
    {
        get
        {
            var version = PlatformUpdateSettings.ReadInstalledPlatformVersion();
            return string.IsNullOrEmpty(version) ? "(알 수 없음)" : version;
        }
    }

    /// <summary>상단 브랜딩 바를 접을지 여부. 체크하면 로고·시계 줄이 통째로 숨어 콘텐츠가 그만큼 넓어진다.</summary>
    public bool TopBarCollapsed
    {
        get => settings.TopBarCollapsed;
        set
        {
            settings.TopBarCollapsed = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 화면에 보여줄 배율. UiSettingsService.BaselineChromeScale을 "100%" 기준으로 삼는다 —
    /// 즉 실제 ChromeScale이 BaselineChromeScale과 같으면 여기선 1.0(100%)으로 보인다.
    /// </summary>
    public double ChromeScalePercent
    {
        get => settings.ChromeScale / UiSettingsService.BaselineChromeScale;
        set
        {
            settings.ChromeScale = value * UiSettingsService.BaselineChromeScale;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            settings.Save();
            Update.Save();
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
            settings.Reload();
            Update.Reload();
            // settings의 프로퍼티는 알아서 변경 통지가 뜨지만, 이 화면이 바인딩하는 건 그 값을 감싼
            // SelectedThemeOption 등 계산 프로퍼티라 여기서 직접 다시 읽으라고 알려줘야 한다.
            OnPropertyChanged(nameof(SelectedThemeOption));
            OnPropertyChanged(nameof(SelectedNavPositionOption));
            OnPropertyChanged(nameof(SelectedToastPositionOption));
            OnPropertyChanged(nameof(TopBarCollapsed));
            OnPropertyChanged(nameof(UpdateEnabled));
            OnPropertyChanged(nameof(UpdateAuto));
            OnPropertyChanged(nameof(UpdateRepositoryUrl));
            OnPropertyChanged(nameof(InstalledPlatformVersion));
            OnPropertyChanged(nameof(ChromeScalePercent));
            MessageUtil.ShowSuccessToast("새로고침되었습니다.");
        }
        catch
        {
            MessageUtil.ShowErrorToast("새로고침하지 못했습니다.");
        }
    }
}
