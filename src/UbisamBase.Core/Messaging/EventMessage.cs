namespace UbisamBase.Core.Messaging;

public sealed class EventMessage
{
    public string Subject { get; }
    public object? Param { get; }
    public string? From { get; }

    public EventMessage(string subject, object? param = null, string? from = null)
    {
        Subject = subject;
        Param = param;
        From = from;
    }
}
