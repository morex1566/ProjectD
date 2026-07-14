using UnityEngine;
using UnityEngine.Serialization;

namespace TRPG.Runtime
{
    /// <summary>
    /// 비플레이 상태에서 생성된 월드 청크를 씬에 미리 표시합니다.
    /// </summary>
    public sealed class WorldGenerationPreviewer : MonoBehaviour
    {
        [SerializeField] private WorldGenerator worldGenerator = new WorldGenerator();

        [SerializeField] private GameObject worldMapInstance = null;

        [FormerlySerializedAs("setup")]
        [SerializeField] private WorldGenerationSettingsData settings;


        /// <summary>
        /// 현재 설정으로 에디터 미리보기용 월드를 생성하고 모든 청크를 표시합니다.
        /// </summary>
        [ContextMenu(nameof(Generate))]
        private void Generate()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("월드 생성 미리보기는 비플레이 상태에서만 사용할 수 있습니다.", this);
                return;
            }

            Clear();

            WorldMap worldMap = worldGenerator.Generate(settings);
            worldMapInstance = WorldManager.RenderWorld(settings, worldMap);

            worldMapInstance.name = "WorldMap_Preview";
            worldMapInstance.hideFlags = HideFlags.DontSave;
            worldMapInstance.transform.SetParent(transform, false);
        }

        /// <summary>
        /// 이전에 생성한 에디터 미리보기 청크 오브젝트를 제거합니다.
        /// </summary>
        [ContextMenu(nameof(Clear))]
        private void Clear()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("월드 생성 미리보기는 비플레이 상태에서만 정리할 수 있습니다.", this);
                return;
            }

            if (worldMapInstance == null)
            {
                return;
            }

            DestroyImmediate(worldMapInstance);
            worldMapInstance = null;
        }
    }
}
