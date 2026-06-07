using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldManager 초기화에 필요한 맵 데이터와 프리팹 참조를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WorldManagerSettings", menuName = "Scriptable Objects/Settings/WorldManager")]
    public partial class WorldManagerSettingsData : ScriptableObject
    {
        [Header(nameof(WorldManagerSettingsData) + ".Setup")]

        [SerializeField] private CreatureController monsterPb = null;

        [SerializeField] private CreatureController playerPb = null;

        [SerializeField] private TileIndicatorController allyTileIndicatorPb = null;

        [SerializeField] private TileIndicatorController enemyTileIndicatorPb = null;

        public CreatureController MonsterPb => monsterPb;

        public CreatureController PlayerPf => playerPb;

        public TileIndicatorController AllyTileIndicatorPb => allyTileIndicatorPb;

        public TileIndicatorController EnemyTileIndicatorPb => enemyTileIndicatorPb;
    }

#if UNITY_EDITOR
    public partial class WorldManagerSettingsData : ScriptableObject
    {
        [Header(nameof(WorldManagerSettingsData) + ".Editor")]
        [SerializeField] AssetReferenceT<MapData> testMapData;

        public AssetReferenceT<MapData> TestMapData => testMapData;
    }
#endif
}
