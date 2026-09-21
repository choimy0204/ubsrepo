using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UbisamBase.Core.Capture;
using UbisamBase.Core.Locking;
using UbisamBase.Core.Logging;
using UbisamBase.Core.Messaging;
using UbisamBase.Core.Modules;
using UbisamBase.Core.Settings;

namespace UbisamBase.Core.Shell;

public partial class ShellWindow : Window
{
    /// <summary>전역 탈출 단축키(Ctrl+Shift+F, FullscreenHotkeyInstaller)가 어느 창에서 눌리든
    /// 이 인스턴스에 닿을 수 있도록 하는 참조. 앱에 Shell은 하나뿐이다.</summary>
    internal static ShellWindow? Instance { get; private set; }

    private readonly ShellViewModel viewModel;
    private MainTabViewModel? lastRealTab;
    private bool suppressSelectionSync;
    private bool isKiosk;
    private bool closeApproved;
    private bool closeAsking;

    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        Instance = this;
        this.viewModel = viewModel;
        DataContext = viewModel;

        viewModel.UiSettings.PropertyChanged += OnUiSettingsChanged;
        ApplyNavPosition(viewModel.UiSettings.NavPosition);
        ApplyToastPosition(viewModel.UiSettings.ToastPosition);

        // 하단 탭 바 = 등록된 실제 탭(MainTabs, 왼쪽에 모임) + "화면 잠금"/"바탕화면"/"종료" 같은
        // 동작형 항목(오른쪽 끝에 붙음, ShellStyles.xaml의 DockPanel.Dock=Right 트리거로 처리).
        // DockPanel은 자식을 등록 순서대로 오른쪽부터 채우므로, 화면에 보일 순서의 정반대 순서로
        // 넣어야 한다 — 맨 먼저 넣는 항목이 실제 오른쪽 가장자리를 차지하기 때문이다.
        // 스크린샷/로그/창모드로는 하단이 아니라 상단 브랜딩 바 버튼(ScreenshotButton 등, XAML)으로 있다.
        var composite = new CompositeCollection
        {
            new CollectionContainer { Collection = viewModel.MainTabs },
            // Shutdown()이 아니라 Close()다 — 그래야 ICloseGuard(저장 안 한 변경 확인)를 거친다.
            new NavActionItem("종료", UbisamBase.Core.Modules.Icon.Power, Close),
            new NavActionItem("바탕화면", UbisamBase.Core.Modules.Icon.Desktop, () => WindowState = WindowState.Minimized),
            new NavActionItem("화면 잠금", UbisamBase.Core.Modules.Icon.Lock, () => LockConfirmDialog.ShowAndLock())
        };
        NavList.ItemsSource = composite;

        lastRealTab = viewModel.SelectedMainTab;
        NavList.SelectedItem = viewModel.SelectedMainTab;
        NavList.SelectionChanged += NavList_SelectionChanged;

        NavList.SelectionChanged += (_, _) => MoveTabIndicator(animate: true);
        NavList.Loaded += (_, _) => MoveTabIndicator(animate: false);
        NavList.SizeChanged += (_, _) => MoveTabIndicator(animate: false);
    }

    /// <summary>닫기 전에 열려 있는 모듈 화면에 "지금 닫아도 되는지" 물어본다(ICloseGuard).
    /// 묻는 일이 비동기라 일단 닫기를 취소해 두고, 전부 동의하면 그때 다시 Close()를 부른다.
    /// 모듈이 셸 창의 Closing에 직접 붙어 각자 Cancel/Close를 거는 충돌을 막기 위한 단일 통로다.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel || closeApproved)
        {
            return;
        }

        if (closeAsking)
        {
            // 이미 묻고 있는 중이면 그 답을 기다린다 — 중복으로 묻지 않는다.
            e.Cancel = true;
            return;
        }

        var guards = CollectCloseGuards();
        if (guards.Count == 0)
        {
            return;
        }

        e.Cancel = true;
        _ = AskCloseGuardsAsync(guards);
    }

    /// <summary>셸 창이 닫히면 앱도 끝난다 — 로그 창 같은 보조 창이 떠 있어도 남지 않도록 명시적으로 종료한다.</summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current?.Shutdown();
    }

    private async Task AskCloseGuardsAsync(List<ICloseGuard> guards)
    {
        closeAsking = true;
        try
        {
            foreach (var guard in guards)
            {
                if (!await guard.CanCloseAsync())
                {
                    return;
                }
            }

            closeApproved = true;
            Close();
        }
        catch (Exception ex)
        {
            // 확인 과정이 깨졌다고 창을 못 닫게 두면 사용자가 앱을 끌 방법이 없어진다.
            new Logger("Shell").E("닫기 확인 중 오류 — 그대로 닫는다", ex);
            closeApproved = true;
            Close();
        }
        finally
        {
            closeAsking = false;
        }
    }

    /// <summary>이미 열린 적 있는 탭 화면(과 그 DataContext) 중 ICloseGuard를 구현한 것만 모은다.
    /// 아직 한 번도 안 연 탭은 만들지 않는다 — 물어보려고 화면을 새로 띄우는 꼴이 되기 때문.</summary>
    private List<ICloseGuard> CollectCloseGuards()
    {
        var guards = new List<ICloseGuard>();

        foreach (var tab in viewModel.MainTabs)
        {
            AddCloseGuard(guards, tab.CreatedContent);

            foreach (var sub in tab.SubTabs)
            {
                AddCloseGuard(guards, sub.CreatedContent);
            }
        }

        return guards;
    }

    private static void AddCloseGuard(List<ICloseGuard> guards, object? content)
    {
        if (content == null)
        {
            return;
        }

        if (content is ICloseGuard guard && !guards.Contains(guard))
        {
            guards.Add(guard);
        }

        if (content is FrameworkElement element && element.DataContext is ICloseGuard viewModelGuard &&
            !guards.Contains(viewModelGuard))
        {
            guards.Add(viewModelGuard);
        }
    }

    /// <summary>창 핸들이 생겨야 모니터 판별(MonitorHelper)이 가능하므로, 시작 시 kiosk 배치는
    /// 여기서 한다(생성자 시점엔 아직 핸들이 없을 수 있음).</summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyKiosk();
    }

    /// <summary>작업표시줄까지 덮는 전체화면. Topmost는 걸지 않는다 — 현장의 다른 경보/알림 창이
    /// 이 화면에 가려지면 안 된다는 요구 때문(사용자 결정). 대신 탈출은 이 버튼 또는
    /// Ctrl+Shift+F(FullscreenHotkeyInstaller)로 항상 가능하다.</summary>
    private void ApplyKiosk()
    {
        isKiosk = true;
        var bounds = MonitorHelper.GetBounds(this);
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Normal;
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        Topmost = false;
        FitToMonitor(bounds);

        // 버튼 아이콘/툴팁은 "다음에 누르면 어떻게 되는지"를 나타낸다 — 지금 kiosk이므로 다음 동작은 창모드 전환.
        SetFullscreenButtonState(toWindowed: true);
    }

    /// <summary>모니터 경계를 그대로 Width/Height(DIP)에 넣어도 환경에 따라 창이 몇 px 더 크게
    /// 만들어져(DPI 배율 반올림 등) 오른쪽·아래가 화면 밖으로 밀리고, 그만큼 하단 네비게이션 바가
    /// 잘려 보인다. 그래서 만들어진 창을 실측해 모니터와의 차이만큼 되돌린다.
    /// 실측값과 목표값은 둘 다 Win32 좌표계라, 그 비율로 DIP 환산까지 함께 처리된다.</summary>
    private void FitToMonitor(Rect bounds)
    {
        var actual = MonitorHelper.GetWindowBounds(this);
        if (actual.IsEmpty || actual.Width <= 0 || actual.Height <= 0 || Width <= 0 || Height <= 0)
        {
            return;
        }

        var pixelsPerDipX = actual.Width / Width;
        var pixelsPerDipY = actual.Height / Height;
        if (pixelsPerDipX <= 0 || pixelsPerDipY <= 0)
        {
            return;
        }

        Width = bounds.Width / pixelsPerDipX;
        Height = bounds.Height / pixelsPerDipY;
        Left -= (actual.Left - bounds.Left) / pixelsPerDipX;
        Top -= (actual.Top - bounds.Top) / pixelsPerDipY;
    }

    /// <summary>이동/리사이즈 가능한 일반 창. WindowStyle은 계속 None(프레임리스, 브랜딩 바와
    /// 일관)이지만 ResizeMode=CanResizeWithGrip이면 WPF가 가장자리에 OS 리사이즈 히트영역을
    /// 그대로 붙여준다 — 직접 구현할 필요가 없다. 이동은 브랜딩 바 드래그(BrandBar_MouseLeftButtonDown)로.</summary>
    private void ApplyWindowed()
    {
        isKiosk = false;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        WindowState = WindowState.Normal;
        Width = 1200;
        Height = 760;

        var work = SystemParameters.WorkArea;
        Left = work.Left + Math.Max(0, (work.Width - Width) / 2);
        Top = work.Top + Math.Max(0, (work.Height - Height) / 2);
        Topmost = false;

        SetFullscreenButtonState(toWindowed: false);
    }

    /// <summary>버튼 툴팁("모드 변경")은 고정이고, 아이콘만 "다음 클릭이 어느 방향인지"를 나타낸다 —
    /// toWindowed=true(지금 kiosk)면 창모드로 나가는 아이콘을, false(지금 창모드)면 전체화면으로
    /// 들어가는 아이콘을 보여준다.</summary>
    private void SetFullscreenButtonState(bool toWindowed)
    {
        FullscreenIconPath.Data = (Geometry)FindResource(toWindowed ? "Ubisam.Icon.FullscreenExit" : "Ubisam.Icon.Fullscreen");
    }

    /// <summary>같은 버튼(또는 Ctrl+Shift+F)으로 kiosk ↔ 창모드를 오간다.</summary>
    internal void ToggleFullscreen()
    {
        try
        {
            if (isKiosk)
            {
                ApplyWindowed();
            }
            else
            {
                ApplyKiosk();
            }
        }
        catch (Exception ex)
        {
            // 전환 중 예외가 나도 조작 불가 상태(중간 프레임)로 남지 않도록 창모드로 안전 폴백한다.
            new UbisamBase.Core.Logging.Logger("Shell").E("전체화면 전환 실패", ex);
            ResizeMode = ResizeMode.CanResizeWithGrip;
            WindowState = WindowState.Normal;
            Topmost = false;
            isKiosk = false;
            SetFullscreenButtonState(toWindowed: false);
        }
    }

    /// <summary>창모드에서만 브랜딩 바를 끌어 창을 옮길 수 있다. kiosk에서는 화면 전체를 채우고 있어
    /// 이동이 의미가 없으므로 무시한다.</summary>
    private void BrandBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!isKiosk)
        {
            DragMove();
        }
    }

    private void ScreenshotButton_Click(object sender, RoutedEventArgs e) => ScreenshotService.Capture(this);

    private void LogButton_Click(object sender, RoutedEventArgs e) => LogViewerWindow.ShowOrFocus(this);

    private void FullscreenButton_Click(object sender, RoutedEventArgs e) => ToggleFullscreen();

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (suppressSelectionSync)
        {
            return;
        }

        if (NavList.SelectedItem is NavActionItem action)
        {
            action.Execute();

            // 동작형 항목은 화면 전환이 아니므로 원래 선택돼 있던 실제 탭으로 되돌린다.
            suppressSelectionSync = true;
            NavList.SelectedItem = lastRealTab;
            suppressSelectionSync = false;
        }
        else if (NavList.SelectedItem is MainTabViewModel tab)
        {
            lastRealTab = tab;
            viewModel.SelectedMainTab = tab;
        }
    }

    private void OnUiSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UiSettingsService.NavPosition))
        {
            ApplyNavPosition(((UiSettingsService)sender!).NavPosition);
        }
        else if (e.PropertyName == nameof(UiSettingsService.ToastPosition))
        {
            ApplyToastPosition(((UiSettingsService)sender!).ToastPosition);
        }
    }

    /// <summary>토스트 오버레이의 정렬/여백을 UiSettings.ToastPosition에 맞춘다. ToastList는
    /// 콘텐츠와 같은 Grid Row에 있어서(ShellWindow.xaml) 상단 브랜딩 바·하단 서브탭 줄과는
    /// 애초에 겹치지 않으므로, 여백은 콘텐츠 영역 안쪽 여유만 두면 된다.</summary>
    private void ApplyToastPosition(ToastPosition position)
    {
        var isLeft = position is ToastPosition.TopLeft or ToastPosition.BottomLeft;
        var isTop = position is ToastPosition.TopLeft or ToastPosition.TopRight;

        ToastList.HorizontalAlignment = isLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        ToastList.VerticalAlignment = isTop ? VerticalAlignment.Top : VerticalAlignment.Bottom;
        ToastList.Margin = new Thickness(20);

        // 상단 배치는 새 토스트가 목록 앞(=쌓임의 시작점=앵커에 가장 가까운 자리)에 와야 위에 뜨고,
        // 하단 배치는 StackPanel이 항상 위→아래로 쌓기 때문에 뒤에 추가하는 것만으로 이미 앵커(화면 하단)에
        // 가장 가깝게 붙는다 — 그래서 위치가 바뀔 때마다 쌓는 방향도 같이 바꿔줘야 한다.
        ToastService.Current.StackNewestFirst = isTop;
    }

    /// <summary>토스트가 새로 뜰 때 배치된 쪽(좌/우) 화면 끝에서 미끄러져 들어오는 애니메이션.</summary>
    private void ToastItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Border border || border.RenderTransform is not TranslateTransform transform)
        {
            return;
        }

        var fromLeft = ToastList.HorizontalAlignment == HorizontalAlignment.Left;
        var distance = border.ActualWidth > 0 ? border.ActualWidth + 60 : 420;
        var startX = fromLeft ? -distance : distance;

        transform.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(startX, 0, new Duration(TimeSpan.FromMilliseconds(260)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });

        // 표시 시간이 끝나면 ToastService가 IsClosing을 켠다 — 그 순간 등장과 같은 방향의 화면 끝으로
        // 다시 미끄러져 나가는 퇴장 애니메이션을 재생한다. 컬렉션에서의 실제 제거는 ToastService가
        // 이 애니메이션 길이만큼 기다렸다가 한다.
        if (border.DataContext is ToastMessage toast)
        {
            PropertyChangedEventHandler? handler = null;
            handler = (_, args) =>
            {
                if (args.PropertyName != nameof(ToastMessage.IsClosing) || !toast.IsClosing)
                {
                    return;
                }

                var exitDistance = border.ActualWidth > 0 ? border.ActualWidth + 60 : 420;
                var endX = fromLeft ? -exitDistance : exitDistance;

                border.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(220))));
                transform.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(0, endX, new Duration(TimeSpan.FromMilliseconds(220)))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                    });

                toast.PropertyChanged -= handler;
            };
            toast.PropertyChanged += handler;

            border.Unloaded += (_, _) => toast.PropertyChanged -= handler;
        }
    }

    private void ApplyNavPosition(NavPosition position)
    {
        DockPanel.SetDock(NavList, position switch
        {
            NavPosition.Right => Dock.Right,
            NavPosition.Bottom => Dock.Bottom,
            _ => Dock.Left
        });

        var isHorizontal = position == NavPosition.Bottom;
        NavList.Style = (Style)FindResource(isHorizontal
            ? "Ubisam.Style.SidebarBar.Horizontal"
            : "Ubisam.Style.SidebarBar.Vertical");

        NavList.Width = isHorizontal ? double.NaN : 140;
        NavList.Height = double.NaN;

        if (isHorizontal)
        {
            Dispatcher.BeginInvoke(new Action(() => MoveTabIndicator(animate: false)));
        }
    }

    /// <summary>하단 탭의 선택 인디케이터를 선택된 탭 아래로 옮긴다. 좌/우 네비게이션일 때는 인디케이터가 없어 무시된다.</summary>
    private void MoveTabIndicator(bool animate)
    {
        if (NavList.Template?.FindName("SelectionIndicator", NavList) is not Border indicator ||
            NavList.Template.FindName("SelectionIndicatorShift", NavList) is not TranslateTransform shift)
        {
            return;
        }

        if (NavList.SelectedItem == null ||
            NavList.ItemContainerGenerator.ContainerFromItem(NavList.SelectedItem) is not ListBoxItem container ||
            !container.IsArrangeValid)
        {
            indicator.Width = 0;
            return;
        }

        var origin = container.TranslatePoint(new Point(0, 0), NavList);
        var targetX = origin.X;
        var targetWidth = container.ActualWidth;

        if (!animate)
        {
            shift.BeginAnimation(TranslateTransform.XProperty, null);
            indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
            shift.X = targetX;
            indicator.Width = targetWidth;
            return;
        }

        var duration = new Duration(TimeSpan.FromMilliseconds(260));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        shift.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(targetX, duration) { EasingFunction = ease, FillBehavior = FillBehavior.HoldEnd });
        indicator.BeginAnimation(FrameworkElement.WidthProperty,
            new DoubleAnimation(targetWidth, duration) { EasingFunction = ease, FillBehavior = FillBehavior.HoldEnd });
    }
}
