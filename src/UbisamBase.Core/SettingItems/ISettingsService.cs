using System.Collections.Generic;

namespace UbisamBase.Core.SettingItems;

public interface ISettingsService
{
    IReadOnlyList<string> RegisteredKeys { get; }

    /// <summary>키로 저장된 설정이 있으면 로드해서, 없으면 defaultInstance를 그대로 등록한다.</summary>
    T Register<T>(string key, T defaultInstance) where T : class, ISettingItem;

    /// <summary>Register와 동일하지만 ISettingItem 구현을 요구하지 않는다 — AddSettingsSub가 임의의 POCO를 등록할 때 쓴다.</summary>
    T RegisterSettings<T>(string key, T defaultInstance) where T : class;

    T Get<T>(string key) where T : class, ISettingItem;

    object GetRaw(string key);

    void Save(string key);

    /// <summary>디스크에 저장된 값으로 다시 불러와 기존 인스턴스에 반영한다(참조는 그대로 유지). "새로고침" 버튼이 호출한다.</summary>
    void Reload(string key);
}
