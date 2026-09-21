using System;
using System.Collections.Generic;
using System.Linq;

namespace UbisamBase.Core.Messaging;

public sealed class EventBus : IEventBus
{
    private readonly List<(string Subject, Action<EventMessage> Handler)> subscribers = new();
    private readonly object gate = new();

    public void Publish(EventMessage message)
    {
        List<Action<EventMessage>> handlers;
        lock (gate)
        {
            handlers = subscribers.Where(s => s.Subject == message.Subject).Select(s => s.Handler).ToList();
        }

        foreach (var handler in handlers)
        {
            handler(message);
        }
    }

    public IDisposable Subscribe(string subject, Action<EventMessage> handler)
    {
        var entry = (subject, handler);
        lock (gate)
        {
            subscribers.Add(entry);
        }

        return new Unsubscriber(() =>
        {
            lock (gate)
            {
                subscribers.Remove(entry);
            }
        });
    }

    private sealed class Unsubscriber : IDisposable
    {
        private readonly Action onDispose;

        public Unsubscriber(Action onDispose) => this.onDispose = onDispose;

        public void Dispose() => onDispose();
    }
}
