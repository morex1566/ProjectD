using UnityEngine;
using UnityEngine.Serialization;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager가 런타임에 사용할 월드 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PlayerManagerSettingsData", menuName = "Scriptable Objects/Settings/Player Manager")]
    public class PlayerManagerSettingsData : ScriptableObject
    {
        public GameObject SelectorPrefab;
    }
}
