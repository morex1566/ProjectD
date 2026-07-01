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
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            WorldManager manager = GetInstance();
            Settings = ResourceManager.GetResource<WorldManagerSettingsData>(UnityConstant.Addressable.Label.Core);

            manager.worldRoot = new GameObject("World");
        }

        public static WorldCameraController GetWorldCameraController()
        {
            return GameObject.FindGameObjectWithTag(UnityConstant.Tags.WorldCamera)?.GetComponent<WorldCameraController>();
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
    }
}
