using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager가 런타임에 사용할 월드 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WorldManagerSettings", menuName = "Scriptable Objects/Settings/World Manager")]
    public class WorldManagerSettingsData : ScriptableObject
    {
        /// <summary>
        /// DataId로 CreatureData를 조회할 시트입니다.
        /// </summary>
        public CreatureSheet CreatureDataSheet;
    }
}
