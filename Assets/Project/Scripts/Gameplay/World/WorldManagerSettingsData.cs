using UnityEngine;
using UnityEngine.Serialization;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager가 런타임에 사용할 월드 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WorldManagerSettings", menuName = "Scriptable Objects/Settings/World Manager")]
    public class WorldManagerSettingsData : ScriptableObject
    {
        /// <summary>
        /// Id로 CreatureData를 조회할 시트입니다.
        /// </summary>
        public CreatureSheet CreatureDataSheet;

        /// <summary>
        /// 월드 설정
        /// </summary>
        public WorldGenerationSettingsData WorldGenerationSettingsData;

        /// <summary>
        /// Id로 WeaponData를 조회할 시트입니다.
        /// </summary>
        [FormerlySerializedAs("ItemDataSheet")]
        public WeaponSheet WeaponDataSheet;
    }
}
