using System;

namespace UbisamBase.Core.Logging;

public sealed class LogEntry
{
    public DateTime Timestamp { get; }
    public string Tag { get; }
    public LogLevel Level { get; }
    public string Message { get; }

    public LogEntry(DateTime timestamp, string tag, LogLevel level, string message)
    {
        Timestamp = timestamp;
        Tag = tag;
        Level = level;
        Message = message;
    }

    public override string ToString()
        => $"{Timestamp:HH:mm:ss.fff} [{Level.ToString()[0]}] {Tag}: {Message}";
}
