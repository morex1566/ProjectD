using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// PlayerManager 런타임에 사용할 리소스 로딩 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PlayerManagerSettings", menuName = "Scriptable Objects/Settings/PlayerManager")]
    public class PlayerManagerSettings : ScriptableObject
    {
        [SerializeField] public GameObject ObjectSelector;

    }
}
