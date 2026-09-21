namespace UbisamBase.Core.SettingItems;

/// <summary>
/// 이 인터페이스를 구현한 POCO 클래스를 ISettingsService.Register로 등록하면
/// 플랫폼 고정 "설정" 탭에서 자동으로 편집 UI가 생기고 JSON으로 저장된다.
/// System.ComponentModel의 표준 속성(DisplayName, Category, ReadOnly, Browsable)을 그대로 쓸 수 있다.
/// </summary>
public interface ISettingItem
{
}
