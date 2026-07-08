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

        /// <summary>
        /// 반복적인 태그 검색 없이 월드 그리드 컨트롤러를 재사용하기 위한 런타임 캐시입니다.
        /// </summary>
        private WorldGridController worldGridController = null;

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = new();


        public static IReadOnlyDictionary<int, CreatureController> Creatures => GetInstance().creatures;


        private void Update()
        {
            // ApplyWorldForcesToCreature();
        }


        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            WorldManager manager = GetInstance();
            Settings = ResourceManager.GetResource<WorldManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            manager.worldRoot = new GameObject("World");
            manager.worldCameraController = null;
            manager.worldGridController = null;
            manager.creatures.Clear();
        }

        /// <summary>
        /// Creature 프리팹을 생성한 뒤 DataId에 해당하는 CreatureData로 컨텍스트를 로드합니다.
        /// </summary>
        public static CreatureController SpawnCreature(GameObject creaturePrefab, string dataId, Vector3 worldPosition)
        {
            if (TryGetCreatureData(dataId, out CreatureData data) == false)
            {
                return null;
            }

            CreatureController creature = SpawnCreature(creaturePrefab, worldPosition);
            if (creature != null)
            {
                creature.LoadContext(data);
            }

            return creature;
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
        public static bool TryGetCreatureData(string dataId, out CreatureData data)
        {
            data = null;

            if (Settings == null || Settings.CreatureDataSheet == null)
            {
                Debug.LogWarning("TryGetCreatureData failed. CreatureDataSheet is not assigned.");
                return false;
            }

            return Settings.CreatureDataSheet.TryGetEntity(dataId, out data);
        }

        /// <summary>
        /// 월드가 등록된 Creature에게 매 프레임 적용할 힘을 처리합니다.
        /// </summary>
        private void ApplyWorldForcesToCreature()
        {
            if (GetWorldGridController() == null)
            {
                return;
            }

            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                ApplyGravityToCreature(pair.Value);
            }
        }

        /// <summary>
        /// Creature의 발 위치를 기준으로 중력 이동과 지면 스냅을 적용합니다.
        /// </summary>
        private void ApplyGravityToCreature(CreatureController creature)
        {
            creature.transform.position += WorldTile.DefaultGravity * Time.deltaTime;
        }

        public static WorldCameraController GetWorldCameraController()
        {
            WorldManager manager = GetInstance();
            if (manager.worldCameraController == null)
            {
                manager.worldCameraController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera)?.GetComponent<WorldCameraController>();
            }

            return manager.worldCameraController;
        }

        public static WorldGridController GetWorldGridController()
        {
            WorldManager manager = GetInstance();
            if (manager.worldGridController == null)
            {
                manager.worldGridController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldGrid)?.GetComponent<WorldGridController>();
            }

            return manager.worldGridController;
        }

        public static WorldTilemapController GetWorldTilemapController(WorldTilemapType worldTilemapType)
        {
            WorldGridController gridController = GetWorldGridController();

            if (gridController == null)
            {
                return null;
            }

            if (gridController.TryGetTilemapController(worldTilemapType, out WorldTilemapController tilemapController) == false)
            {
                return null;
            }

            return tilemapController;
        }

        public static Vector3Int WorldToCell(Vector3 worldPosition)
        {
            WorldGridController gridController = GetWorldGridController();

            if (gridController == null || gridController.Grid == null)
            {
                return Vector3Int.zero;
            }

            return gridController.Grid.WorldToCell(worldPosition);
        }

        public static Vector3 CellToWorld(Vector3Int cellPos)
        {
            WorldGridController gridController = GetWorldGridController();

            if (gridController == null || gridController.Grid == null)
            {
                return Vector3.zero;
            }

            return gridController.Grid.CellToWorld(cellPos);
        }
    }
}
