using System;
using System.Collections.Generic;
using System.Diagnostics;
using UbisamBase.Core.Logging;

namespace UbisamBase.Core.Utils;

/// <summary>
/// Stopwatch를 감싼 단일 타임아웃 항목.
/// Start() 로 시작하고, End() 로 경과시간(ms)을 반환한다.
/// Start/End 시 Logger 로 자동 로그를 남긴다.
/// 모든 상태 변경 메서드는 thread-safe.
/// </summary>
public class TimerUtil
{
    private readonly Stopwatch _sw = new Stopwatch();
    private readonly object _lock = new object();
    private static readonly Logger LOG = new Logger("TimeUtil");

    private long _limitMs = 0;
    private DateTime _startTime = DateTime.MinValue;

    /// <summary>이 타이머의 식별 키 (TimeOutManager 가 주입). 단독 생성 시 비어있을 수 있음.</summary>
    public string Key { get; internal set; } = string.Empty;

    /// <summary>타임아웃 한계(ms). 0 이하이면 무제한.</summary>
    public long LimitMs
    {
        get { lock (_lock) return _limitMs; }
        set { lock (_lock) _limitMs = value; }
    }

    /// <summary>현재 실행 중인지 여부</summary>
    public bool IsRunning
    {
        get { lock (_lock) return _sw.IsRunning; }
    }

    /// <summary>경과 시간 (ms). 멈춘 상태면 마지막 측정값.</summary>
    public long ElapsedMs
    {
        get { lock (_lock) return _sw.ElapsedMilliseconds; }
    }

    /// <summary>경과 시간 (TimeSpan)</summary>
    public TimeSpan Elapsed
    {
        get { lock (_lock) return _sw.Elapsed; }
    }

    /// <summary>마지막 Start() 호출 시각</summary>
    public DateTime StartTime
    {
        get { lock (_lock) return _startTime; }
    }

    public TimerUtil() { }

    public TimerUtil(string key)
    {
        Key = key ?? string.Empty;
    }

    /// <summary>타이머 시작 (이미 동작 중이면 0으로 리셋 후 재시작).</summary>
    public void Start(bool useLog = true)
    {
        lock (_lock)
        {
            _sw.Restart();
            _startTime = DateTime.Now;
        }
        if (useLog)
            LOG.I($"{Key} Timer Start");
    }

    /// <summary>타이머 시작 + 타임아웃 한계(ms) 동시 설정.</summary>
    public void Start(long limitMs, bool useLog = true)
    {
        lock (_lock)
        {
            _limitMs = limitMs;
            _sw.Restart();
            _startTime = DateTime.Now;
        }
        if (useLog)
            LOG.I($"{Key} Timer Start (Limit {limitMs}ms)");
    }

    /// <summary>타이머 정지 후 경과시간(ms)을 반환.</summary>
    public long End(bool useLog = true)
    {
        long ms;
        lock (_lock)
        {
            _sw.Stop();
            ms = _sw.ElapsedMilliseconds;
        }
        if (useLog)
            LOG.I($"{Key} Time : {ms}ms");
        return ms;
    }

    /// <summary>타이머를 멈추지 않고 0으로 초기화.</summary>
    public void Reset()
    {
        lock (_lock) _sw.Reset();
    }

    public DateTime GetStartTime() => StartTime;

    /// <summary>
    /// 동작 중이면서 LimitMs 를 초과했는지 검사.
    /// (LimitMs 가 0 이하이면 항상 false)
    /// </summary>
    public bool IsTimeout()
    {
        lock (_lock)
        {
            if (_limitMs <= 0) return false;
            return _sw.IsRunning && _sw.ElapsedMilliseconds >= _limitMs;
        }
    }

    /// <summary>지정한 한계값(ms)을 초과했는지 검사.</summary>
    public bool IsTimeout(long limitMs)
    {
        lock (_lock)
        {
            return _sw.IsRunning && _sw.ElapsedMilliseconds >= limitMs;
        }
    }

    public override string ToString()
    {
        long ms; long limit;
        lock (_lock)
        {
            ms = _sw.ElapsedMilliseconds;
            limit = _limitMs;
        }
        return $"{Key} {ms} ms" + (limit > 0 ? $" / {limit} ms" : "");
    }
}

/// <summary>
/// 문자열 키 기반 타임아웃 관리자 (thread-safe).
/// 키는 미리 정의할 필요 없이 사용 시점에 자유롭게 입력 가능 (없으면 자동 생성).
/// 플랫폼이 DI 싱글턴으로 등록해두므로, 어떤 ViewModel/서비스든 생성자에 선언하면 같은 인스턴스를 공유한다.
/// 사용 예) _Time["Ch1"].Start();  long ms = _Time["Ch1"].End();
/// </summary>
public class TimeOutManager
{
    private readonly Dictionary<string, TimerUtil> _timers
        = new Dictionary<string, TimerUtil>(StringComparer.Ordinal);
    private readonly object _lock = new object();

    /// <summary>문자열 키 인덱서. 없으면 자동 생성.</summary>
    public TimerUtil this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            lock (_lock)
            {
                if (!_timers.TryGetValue(key, out var item))
                {
                    item = new TimerUtil(key);
                    _timers[key] = item;
                }
                return item;
            }
        }
    }

    /// <summary>등록된 키 목록 (스냅샷).</summary>
    public IEnumerable<string> Keys
    {
        get
        {
            lock (_lock) return new List<string>(_timers.Keys);
        }
    }

    /// <summary>등록된 타이머 개수</summary>
    public int Count
    {
        get { lock (_lock) return _timers.Count; }
    }

    /// <summary>해당 키 존재 여부</summary>
    public bool Contains(string key)
    {
        lock (_lock) return _timers.ContainsKey(key);
    }

    /// <summary>해당 키 제거</summary>
    public bool Remove(string key)
    {
        lock (_lock) return _timers.Remove(key);
    }

    /// <summary>모든 타이머 제거</summary>
    public void Clear()
    {
        lock (_lock) _timers.Clear();
    }

    /// <summary>모든 타이머 정지 + 0으로 리셋 (등록은 유지).</summary>
    public void ResetAll()
    {
        // 스냅샷으로 복사 후 락 밖에서 Reset 호출 → 중첩락 회피
        List<TimerUtil> snapshot;
        lock (_lock)
        {
            snapshot = new List<TimerUtil>(_timers.Values);
        }
        foreach (var t in snapshot)
            t.Reset();
    }
}
