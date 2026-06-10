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

        private static readonly Dictionary<int, IWorldObject> worldInsts = new();

        private static GameObject worldRoot = null;

        public static ObjectSelector Selector = null;

        public static WorldCameraController CamController = null;

        public static CreatureDataSheet creatureDataSheet = null;



        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
        }

        public void Start()
        {
            worldRoot = new GameObject("World");
            Selector = Instantiate(settings.Selector);
            CamController = Instantiate(settings.CamController);
            creatureDataSheet = ResourceManager.GetResource(settings.CreatureDataSheetRef);
        }

        public static IWorldObject Spawn(IdKeyData idKeyData, Vector3 position)
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
            IWorldObject worldObject = worldInsts[instanceId];
            worldInsts.Remove(instanceId);

            if (worldObject is Component component)
            {
                Destroy(component.gameObject);
            }

            return true;
        }

        private static void Register(IWorldObject worldObject)
        {
            worldInsts.Add(worldObject.InstanceId, worldObject);
        }
    }
}
