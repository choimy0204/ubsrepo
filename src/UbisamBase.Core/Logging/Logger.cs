using System;

namespace UbisamBase.Core.Logging;

/// <summary>
/// 클래스마다 `new Logger("태그")`로 만들어 쓰는 가벼운 로거. DI 없이 바로 사용할 수 있다.
/// </summary>
public sealed class Logger
{
    private readonly string tag;

    public Logger(string tag)
    {
        this.tag = tag;
    }

    public void V(string message) => LogService.Current.Write(tag, LogLevel.Verbose, message);
    public void D(string message) => LogService.Current.Write(tag, LogLevel.Debug, message);
    public void I(string message) => LogService.Current.Write(tag, LogLevel.Info, message);
    public void W(string message) => LogService.Current.Write(tag, LogLevel.Warning, message);
    public void E(string message) => LogService.Current.Write(tag, LogLevel.Error, message);
    public void E(string message, Exception ex) => LogService.Current.Write(tag, LogLevel.Error, $"{message} : {ex}");
}
