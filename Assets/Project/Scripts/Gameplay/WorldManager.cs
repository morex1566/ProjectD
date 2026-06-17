using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 시스템 진입점으로서 맵, 크리처, 인디케이터 기능을 중개합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private static WorldManagerSettingsData settings = null;

        private static readonly Dictionary<int, CreatureController> creatures = new();

        private static GameObject worldRoot = null;

        public static Map Map = null;

        public static WorldCameraController CamController = null;

        public static CreatureDataSheet creatureDataSheet = null;



        public static IReadOnlyDictionary<int, CreatureController> Creatures => creatures;

        public static WorldManagerSettingsData Settings => settings;


        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");

            worldRoot = new GameObject("World");
            Map = GameObject.FindGameObjectWithTag(UnityConstant.Tags.Map).GetComponent<Map>();
            CamController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera).GetComponent<WorldCameraController>();
            creatureDataSheet = ResourceManager.GetResource(settings.CreatureDataSheetRef);
        }

        public static IWorldCreature Spawn(IdKeyData idKeyData, Vector3 position)
        {
            CreatureData creatureData = creatureDataSheet.GetCreatureData(idKeyData.Id);
            GameObject instObj = Instantiate(creatureData.CreaturePf, position, Quaternion.identity, worldRoot.transform);
            instObj.name = creatureData.DataId;

            CreatureController creatureController = instObj.GetComponent<CreatureController>();
            creatureController.Init(creatureData);

            Register(creatureController);

            return creatureController;
        }

        public static bool Despawn(int instanceId)
        {
            IWorldCreature worldObject = creatures[instanceId];
            creatures.Remove(instanceId);

            if (worldObject is Component component)
            {
                Destroy(component.gameObject);
            }

            return true;
        }

        private static void Register(CreatureController creature)
        {
            creatures.Add(creature.InstanceId, creature);
        }
    }
}
