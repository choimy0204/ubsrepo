using System.Threading.Tasks;

namespace UbisamBase.Core.Modules;

/// <summary>
/// 저장하지 않은 변경처럼, 모듈이 "지금 닫으면 안 된다"고 말할 수 있는 통로.
/// 모듈의 View나 그 DataContext(ViewModel)가 구현하면 셸이 창을 닫기 전에 물어본다.
///
/// 셸 창의 Closing에 모듈이 직접 붙으면 모듈이 여럿일 때 서로 Cancel/Close를 걸어 충돌하므로,
/// 반드시 이 인터페이스를 쓴다. 이미 만들어진(한 번이라도 열린) 탭 화면만 물어본다.
/// </summary>
public interface ICloseGuard
{
    /// <summary>false를 돌려주면 닫기를 멈춘다. 사용자에게 묻는 것도 이 안에서 한다.
    /// 하나라도 false면 뒤의 화면에는 묻지 않는다.</summary>
    Task<bool> CanCloseAsync();
}
