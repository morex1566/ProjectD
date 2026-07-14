using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 시스템 진입점으로서 맵, 크리처, 인디케이터 기능을 중개합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        public static WorldManagerSettingsData Settings { get; private set; }

        private GameObject worldRoot = null;

        /// <summary>
        /// 반복적인 태그 검색 없이 월드 카메라를 재사용하기 위한 런타임 캐시입니다.
        /// </summary>
        private WorldCameraController worldCameraController = null;

        private WorldMap worldMap = null;

        private GameObject worldMapInstance = null;

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = new();


        public static IReadOnlyDictionary<int, CreatureController> Creatures => GetInstance().creatures;


        public void Start()
        {
            worldCameraController = null;

            // 월드 런타임 루트와 생성 결과를 구성합니다.
            worldRoot = new GameObject("World");
            {
                worldRoot.transform.SetParent(transform, false);
                worldMap = Settings.WorldGenerationSettingsData.WorldGenerator.Generate(Settings.WorldGenerationSettingsData);
                worldMapInstance = RenderWorld(Settings.WorldGenerationSettingsData, worldMap);
                worldMapInstance.transform.SetParent(worldRoot.transform, false);
            }
        }

        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            Settings = ResourceManager.GetResource<WorldManagerSettingsData>(UnityConstant.Addressable.Label.Core);
        }

        /// <summary>
        /// 생성된 월드의 모든 청크를 런타임 오브젝트로 표시합니다.
        /// </summary>
        public static GameObject RenderWorld(WorldGenerationSettingsData settings, WorldMap map)
        {
            GameObject worldMapInstance = new GameObject("WorldMap");
            Grid grid = worldMapInstance.AddComponent<Grid>();
            {
                int tileWorldSize = settings.TilesPerUnit;
                grid.cellSize = new Vector3(tileWorldSize, tileWorldSize, 1f);
            }

            foreach (WorldChunk chunk in map.Chunks.Values)
            {
                CreateChunkInstance(worldMapInstance, settings, chunk);
            }

            return worldMapInstance;
        }

        /// <summary>
        /// 청크 렌더러를 월드 루트 아래에 생성하고 청크 좌표에 맞게 배치합니다.
        /// </summary>
        private static void CreateChunkInstance(GameObject worldMapInstance, WorldGenerationSettingsData settings, WorldChunk chunk)
        {
            GameObject chunkInstance = new GameObject($"Chunk_{chunk.Coordinate.x}_{chunk.Coordinate.y}");
            chunkInstance.transform.SetParent(worldMapInstance.transform, false);

            // 청크의 좌측 하단이 청크 월드 좌표와 일치하도록 배치합니다.
            float chunkWorldSize = settings.TilesPerUnit * settings.TilesPerChunk;
            chunkInstance.transform.localPosition = new Vector3(
                chunk.Coordinate.x * chunkWorldSize,
                chunk.Coordinate.y * chunkWorldSize,
                0f);

            WorldChunkRenderer chunkRenderer = chunkInstance.AddComponent<WorldChunkRenderer>();
            chunkRenderer.Render(chunk, settings);
        }

        /// <summary>
        /// 월드 런타임 오브젝트와 캐시를 정리합니다.
        /// </summary>
        protected override void OnDestroy()
        {
            if (worldRoot != null)
            {
                UnityEngine.Object.Destroy(worldRoot);
            }

            worldRoot = null;
            worldCameraController = null;
            worldMap = null;
            worldMapInstance = null;
            creatures.Clear();
            Settings = null;

            base.OnDestroy();
        }

        /// <summary>
        /// Id에 해당하는 CreatureData의 완성 프리팹을 월드에 생성합니다.
        /// </summary>
        public static CreatureController SpawnCreature(string id, Vector3 worldPosition)
        {
            if (TryGetCreatureData(id, out CreatureData data) == false)
            {
                return null;
            }

            if (data.Prefab == null)
            {
                Debug.LogWarning($"SpawnCreature failed. Creature prefab is not assigned. Id: {id}");
                return null;
            }

            return SpawnCreature(data.Prefab, worldPosition);
        }

        /// <summary>
        /// Creature 프리팹을 월드에 생성합니다.
        /// </summary>
        public static CreatureController SpawnCreature(GameObject creaturePrefab, string id, Vector3 worldPosition)
        {
            if (TryGetCreatureData(id, out CreatureData data) == false)
            {
                return null;
            }

            GameObject prefab = creaturePrefab != null ? creaturePrefab : data.Prefab;
            return SpawnCreature(prefab, worldPosition);
        }

        /// <summary>
        /// Creature 프리팹을 월드에 생성하고 GameObject InstanceID 기준으로 등록합니다.
        /// </summary>
        public static CreatureController SpawnCreature(GameObject creaturePrefab, Vector3 worldPosition)
        {
            if (creaturePrefab == null)
            {
                Debug.LogWarning("SpawnCreature failed. Creature prefab is null.");
                return null;
            }

            if (creaturePrefab.GetComponent<CreatureController>() == null)
            {
                Debug.LogWarning("SpawnCreature failed. Prefab is not creature.");
                return null;
            }

            WorldManager manager = GetInstance();
            Transform parent = manager.worldRoot != null ? manager.worldRoot.transform : manager.transform;
            CreatureController creature = Instantiate(creaturePrefab, worldPosition, Quaternion.identity, parent).GetComponent<CreatureController>();

            manager.creatures[creature.gameObject.GetInstanceID()] = creature;
            return creature;
        }

        /// <summary>
        /// 등록된 Creature를 GameObject InstanceID 기준으로 제거합니다.
        /// </summary>
        public static bool DespawnCreature(int gameObjectInstanceId)
        {
            WorldManager manager = GetInstance();

            if (manager.creatures.TryGetValue(gameObjectInstanceId, out CreatureController creature) == false)
            {
                return false;
            }

            manager.creatures.Remove(gameObjectInstanceId);

            if (creature != null)
            {
                UnityEngine.Object.Destroy(creature.gameObject);
            }

            return true;
        }

        /// <summary>
        /// 등록된 Creature를 제거합니다.
        /// </summary>
        public static bool DespawnCreature(CreatureController creature)
        {
            if (creature == null)
            {
                return false;
            }

            return DespawnCreature(creature.gameObject.GetInstanceID());
        }

        /// <summary>
        /// 월드 설정에 연결된 CreatureSheet에서 CreatureData를 조회합니다.
        /// </summary>
        public static bool TryGetCreatureData(string id, out CreatureData data)
        {
            data = null;

            if (Settings == null || Settings.CreatureDataSheet == null)
            {
                Debug.LogWarning("TryGetCreatureData failed. CreatureDataSheet is not assigned.");
                return false;
            }

            return Settings.CreatureDataSheet.TryGetEntity(id, out data);
        }

        /// <summary>
        /// 월드 설정에 연결된 WeaponSheet에서 WeaponData를 조회합니다.
        /// </summary>
        public static bool TryGetWeaponData(string id, out WeaponData data)
        {
            data = null;

            if (Settings == null || Settings.WeaponDataSheet == null)
            {
                Debug.LogWarning("TryGetWeaponData failed. WeaponDataSheet is not assigned.");
                return false;
            }

            return Settings.WeaponDataSheet.TryGetEntity(id, out data);
        }

        /// <summary>
        /// 이전 Item 명칭으로 작성된 호출부와의 호환을 유지합니다.
        /// </summary>
        public static bool TryGetItemData(string id, out WeaponData data)
        {
            return TryGetWeaponData(id, out data);
        }

        /// <summary>
        /// Id에 해당하는 WeaponData의 완성 프리팹을 월드에 생성합니다.
        /// </summary>
        public static WeaponController SpawnWeapon(string id, Vector3 worldPosition)
        {
            if (TryGetWeaponData(id, out WeaponData data) == false)
            {
                return null;
            }

            if (data.Prefab == null)
            {
                Debug.LogWarning($"SpawnWeapon failed. Weapon prefab is not assigned. Id: {id}");
                return null;
            }

            return SpawnWeapon(data.Prefab, worldPosition);
        }

        /// <summary>
        /// Weapon 프리팹을 월드에 생성합니다.
        /// </summary>
        public static WeaponController SpawnWeapon(GameObject weaponPrefab, Vector3 worldPosition)
        {
            if (weaponPrefab == null)
            {
                Debug.LogWarning("SpawnWeapon failed. Weapon prefab is null.");
                return null;
            }

            if (weaponPrefab.GetComponent<WeaponController>() == null)
            {
                Debug.LogWarning("SpawnWeapon failed. Prefab is not weapon.");
                return null;
            }

            WorldManager manager = GetInstance();
            Transform parent = manager.worldRoot != null ? manager.worldRoot.transform : manager.transform;
            WeaponController weapon = Instantiate(weaponPrefab, worldPosition, Quaternion.identity, parent).GetComponent<WeaponController>();

            return weapon;
        }

        public static WorldCameraController GetWorldCameraController()
        {
            if (TryGetInstance(out WorldManager manager) == false)
            {
                return null;
            }

            if (manager.worldCameraController == null)
            {
                manager.worldCameraController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera)?.GetComponent<WorldCameraController>();
            }

            return manager.worldCameraController;
        }
    }
}
