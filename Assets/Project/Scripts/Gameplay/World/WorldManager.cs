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

        [SerializeField, ReadOnly] private SerializableDictionary<int, CreatureController> creatures = new();


        public static IReadOnlyDictionary<int, CreatureController> Creatures => GetInstance().creatures.ReadOnlyDictionary;



        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            WorldManager manager = GetInstance();
            Settings = ResourceManager.GetResource<WorldManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            manager.worldRoot = new GameObject("World");
            manager.creatures.Clear();
        }

        /// <summary>
        /// Creature 프리팹을 월드에 생성하고 GameObject InstanceID 기준으로 등록합니다.
        /// </summary>
        public static CreatureController SpawnCreature(GameObject creaturePf, Vector3 worldPos)
        {
            if (creaturePf == null)
            {
                Debug.LogWarning("SpawnCreature failed. Creature prefab is null.");
                return null;
            }

            if (creaturePf.GetComponent<CreatureController>() == null)
            {
                Debug.LogWarning("SpawnCreature failed. Prefab is not creature.");
                return null;
            }

            WorldManager manager = GetInstance();
            Transform parent = manager.worldRoot != null ? manager.worldRoot.transform : manager.transform;
            CreatureController creature = Instantiate(creaturePf, worldPos, Quaternion.identity, parent).GetComponent<CreatureController>();

            RegisterCreature(creature);
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
        /// Creature를 GameObject InstanceID 기준으로 등록합니다.
        /// </summary>
        private static void RegisterCreature(CreatureController creature)
        {
            if (creature == null)
            {
                return;
            }

            WorldManager manager = GetInstance();
            manager.creatures.SetValue(creature.gameObject.GetInstanceID(), creature);
        }

        public static WorldCameraController GetWorldCameraController()
        {
            return GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera)?.GetComponent<WorldCameraController>();
        }

        public static WorldGridController GetWorldGridController()
        {
            WorldGridController gridController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldGrid)?.GetComponent<WorldGridController>();

            if (gridController == null)
            {
                return null;
            }

            return gridController;
        }

        public static WorldGridContext GetWorldGridContext()
        {
            WorldGridContext gridContext = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldGrid)?.GetComponent<WorldGridContext>();

            if (gridContext == null)
            {
                return null;
            }

            return gridContext;
        }

        public static WorldTilemapContext GetWorldTilemapContext(WorldTilemapType worldTilemapType)
        {
            WorldGridContext gridContext = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldGrid)?.GetComponent<WorldGridContext>();

            if (gridContext == null)
            {
                return null;
            }

            if (gridContext.TilemapContextMap.TryGetValue(worldTilemapType, out WorldTilemapContext tilemapContext) == false)
            {
                return null;
            }

            return tilemapContext;
        }

        public static Vector3Int WorldToCell(Vector3 worldPos)
        {
            WorldGridContext gridContext = GetWorldGridContext();

            if (gridContext == null)
            {
                return Vector3Int.zero;
            }

            return gridContext.Grid.WorldToCell(worldPos);
        }

        public static Vector3 CellToWorld(Vector3Int cellPos)
        {
            WorldGridContext gridContext = GetWorldGridContext();

            if (gridContext == null)
            {
                return Vector3.zero;
            }

            return gridContext.Grid.CellToWorld(cellPos);
        }
    }
}
