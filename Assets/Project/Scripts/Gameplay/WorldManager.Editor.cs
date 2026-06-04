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
                Debug.LogWarning("LoadTestMapData is only available in Trigger Mode.");
                return;
            }

            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
            var awaiter = ResourceManager.LoadAsync(UnityConstant.Addressable.Label.Core).GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                if (!Application.isPlaying) return;

                MapData testMapData = ResourceManager.GetResource(settings.TestMapData);
                SpawnTiles(testMapData);
                SpawnMonsters(testMapData);
            });
        }

        /// <summary>
        /// WorldManagerSettings의 TestMapData를 로드
        /// </summary>
        [MenuItem("TRPG/WorldManager/PlayerSpawn()")]
        private static void LoadPlayer()
        {
            SpawnPlayer(Vector3Int.zero);
        }
    }
#endif
}
