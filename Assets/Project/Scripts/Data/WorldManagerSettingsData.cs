using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    /// <summary>
    /// 캐시 스톤스 ID와 월드 생성 프리팹을 연결합니다.
    /// </summary>
    [Serializable]
    public class WorldInstancePrefabEntry
    {
        [SerializeField] private string cacheStonesId = string.Empty;

        [SerializeField] private GameObject prefab = null;

        public string CacheStonesId => cacheStonesId;

        public GameObject Prefab => prefab;
    }

    /// <summary>
    /// WorldManager 초기화에 필요한 맵 데이터와 프리팹 참조를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WorldManagerSettings", menuName = "Scriptable Objects/Settings/WorldManager")]
    public partial class WorldManagerSettingsData : ScriptableObject
    {
        [Header(nameof(WorldManagerSettingsData) + ".World Instances")]

        [SerializeField] private List<WorldInstancePrefabEntry> worldInstancePrefabs = new();

        public IReadOnlyList<WorldInstancePrefabEntry> WorldInstancePrefabs => worldInstancePrefabs;

        public bool TryGetWorldInstancePrefab(string cacheStonesId, out GameObject prefab)
        {
            prefab = null;

            if (string.IsNullOrWhiteSpace(cacheStonesId)) return false;

            foreach (WorldInstancePrefabEntry entry in worldInstancePrefabs)
            {
                if (entry == null) continue;
                if (entry.CacheStonesId != cacheStonesId) continue;

                prefab = entry.Prefab;
                return prefab != null;
            }

            return false;
        }
    }
}
