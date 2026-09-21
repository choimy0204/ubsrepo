using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace UbisamBase.Core.Messaging;

/// <summary>
/// 잠깐 떴다 사라지는 토스트 메시지 큐. ShellWindow가 이 컬렉션을 그려주고, 위치는
/// UiSettings.ToastPosition을 따른다.
/// </summary>
public sealed class ToastService
{
    /// <summary>퇴장 슬라이드 애니메이션(ShellWindow.ToastItem_Loaded)이 끝날 시간을 벌어주고 나서
    /// 실제로 컬렉션에서 제거한다 — 애니메이션 길이(220ms)보다 살짝 여유를 둔다.</summary>
    private static readonly TimeSpan ExitAnimationGrace = TimeSpan.FromMilliseconds(260);

    public static ToastService Current { get; } = new();

    public ObservableCollection<ToastMessage> Messages { get; } = new();

    /// <summary>true면 새 토스트를 목록 맨 앞에 꽂는다(상단 배치용 — 위쪽 화면 끝에 가장 가깝게 쌓임).
    /// false(기본)면 맨 뒤에 붙인다(하단 배치용 — 아래쪽 화면 끝에 가장 가깝게 쌓임).
    /// ShellWindow가 UiSettings.ToastPosition을 따라 갱신한다.</summary>
    public bool StackNewestFirst { get; set; }

    private ToastService()
    {
    }

    public void Show(string text, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        var toast = new ToastMessage(text, type);
        var app = Application.Current;
        if (app == null)
        {
            return;
        }

        app.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (StackNewestFirst)
            {
                Messages.Insert(0, toast);
            }
            else
            {
                Messages.Add(toast);
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();

                // 바로 지우지 않고 먼저 퇴장 애니메이션을 켠다 — ShellWindow가 IsClosing 변화를
                // 감지해 밖으로 미끄러지는 걸 재생한 뒤, 그게 끝날 때쯤 실제로 컬렉션에서 뺀다.
                toast.IsClosing = true;

                var closeTimer = new DispatcherTimer { Interval = ExitAnimationGrace };
                closeTimer.Tick += (_, _) =>
                {
                    closeTimer.Stop();
                    Messages.Remove(toast);
                };
                closeTimer.Start();
            };
            timer.Start();
        }));
    }
}
