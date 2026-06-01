using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
#if UNITY_EDITOR
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        /// <summary>
        /// WorldManagerSettings의 TestMapData를 로드
        /// </summary>
        [MenuItem("TRPG/WorldManager/LoadTestMapData()")]
        private static void LoadTestMapData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("LoadTestMapData is only available in Play Mode.");
                return;
            }

            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
            var awaiter = ResourceManager.LoadAsync(UnityConstant.Addressable.Label.Core).GetAwaiter();

            UnloadMapData();

            awaiter.OnCompleted(() =>
            {
                if (!Application.isPlaying) return;

                MapData testMapData = ResourceManager.GetResource(settings.TestMapData);
                LoadMapData(testMapData);
            });
        }

        /// <summary>
        /// 플레이 모드에서만 테스트 맵 로드 메뉴를 활성화합니다.
        /// </summary>
        [MenuItem("TRPG/WorldManager/LoadTestMapData()", true)]
        private static bool CanLoadTestMapData()
        {
            return Application.isPlaying;
        }
    }
#endif
}
