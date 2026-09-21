using System;

namespace UbisamBase.Core.Messaging;

/// <summary>
/// 문자열 subject 기반의 가벼운 Pub/Sub. 여러 화면/서비스가 서로 직접 참조하지 않고
/// 이벤트로만 통신하고 싶을 때 쓴다.
/// </summary>
public interface IEventBus
{
    void Publish(EventMessage message);

    /// <summary>subject를 구독한다. 반환된 IDisposable을 Dispose하면 구독이 해제된다.</summary>
    IDisposable Subscribe(string subject, Action<EventMessage> handler);
}
