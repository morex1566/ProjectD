using System;
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

        private static readonly Dictionary<int, GameObject> worldInsts = new();

        private static GameObject worldRoot = null;

        public static ObjectSelector Selector = null;

        public static WorldCameraController CamController = null;


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
        }
    }
}
