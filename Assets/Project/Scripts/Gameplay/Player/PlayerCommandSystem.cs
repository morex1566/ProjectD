using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 입력이 해석되는 현재 명령 모드입니다.
    /// </summary>
    public enum PlayerCommandSystemMode
    {
        Idle,
        Construction,
        Mining,
    }

    /// <summary>
    /// 현재 모드에 따라 우클릭을 해석하는 객체
    /// </summary>
    public abstract class PlayerCommandSystem
    {
        public abstract void HandleRightClickPerformed();
    }
}
