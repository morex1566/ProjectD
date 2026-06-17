
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// PlayerManager 초기화에 필요한 플레이어 제어 설정 값을 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PlayerManagerSettings", menuName = "Scriptable Objects/Settings/PlayerManager")]
    public class PlayerManagerSettingsData : ScriptableObject
    {
        [SerializeField] public GameObject SelectorPf;

        [SerializeField] public GameObject DigSystemPf;
    }
}
