namespace UbisamBase.Core.Logging;

/// <summary>로그 뷰어의 "최소 레벨" 선택지 하나. Level이 null이면 "ALL" — 실제 LogEntry.Level에는
/// 없는 값이라 필터 전용으로만 쓴다(내부적으로는 Verbose를 문턱값으로 써서 전부 통과시킨다).</summary>
public sealed class LogLevelFilterOption
{
    public LogLevel? Level { get; }
    public string Label { get; }

    public LogLevelFilterOption(LogLevel? level, string label)
    {
        Level = level;
        Label = label;
    }

    public override string ToString() => Label;
}
