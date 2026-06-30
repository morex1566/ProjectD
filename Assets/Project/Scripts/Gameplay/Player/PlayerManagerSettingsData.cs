using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// PlayerManager가 런타임에 사용할 프리팹 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PlayerManagerSettings", menuName = "Scriptable Objects/Settings/Player Manager")]
    public class PlayerManagerSettingsData : ScriptableObject
    {
        /// <summary>
        /// 플레이어 선택 기능을 담당하는 프리팹입니다.
        /// </summary>
        public GameObject SelectorPf;
    }
}
