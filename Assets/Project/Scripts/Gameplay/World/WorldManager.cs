using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 시스템 진입점으로서 맵, 크리처, 인디케이터 기능을 중개합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private static readonly Dictionary<int, CreatureController> creatures = new();

        private static GameObject worldRoot = null;

        public static MapController MapController = null;

        public static WorldCameraController CamController = null;

        public static CreatureSheet creatureDataSheet = null;



        public static IReadOnlyDictionary<int, CreatureController> Creatures => creatures;


        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            worldRoot = new GameObject("World");
            MapController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.Map).GetComponent<MapController>();
            CamController = GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera).GetComponent<WorldCameraController>();
        }
    }
}
