using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 생성된 월드 청크를 씬에서 미리 표시합니다.
    /// </summary>
    public sealed class WorldGenerationPreviewer : MonoBehaviour
    {
        [SerializeField] private Vector2Int chunkSize = new Vector2Int(5, 5);

        [SerializeField] private WorldGenerator worldGenerator = new WorldGenerator();

        [SerializeField] private WorldSetup setup;


        private void Start()
        {
            Generate();
        }


        /// <summary>
        /// 현재 설정으로 월드를 생성하고 모든 청크를 표시합니다.
        /// </summary>
        [ContextMenu(nameof(Generate))]
        public void Generate()
        {
            Clear();

            WorldMap worldMap = worldGenerator.Generate(chunkSize);
            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                CreateChunkObject(chunk);
            }
        }

        /// <summary>
        /// 이전에 생성한 청크 오브젝트를 제거합니다.
        /// </summary>
        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                Transform child = transform.GetChild(childIndex);

                if (child.GetComponent<WorldChunkRenderer>() != null)
                {
                    DestroyGeneratedObject(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 청크 렌더러를 생성하고 청크 좌표에 맞게 배치합니다.
        /// </summary>
        private void CreateChunkObject(WorldChunk chunk)
        {
            GameObject chunkObject = new GameObject($"Chunk_{chunk.Coordinate.x}_{chunk.Coordinate.y}");
            {
                chunkObject.hideFlags = HideFlags.DontSave;
                chunkObject.transform.SetParent(transform, false);
            }

            WorldChunkRenderer chunkRenderer = chunkObject.AddComponent<WorldChunkRenderer>();
            {
                chunkRenderer.Render(chunk, setup);
            }

            float chunkWorldSize = WorldChunk.Size * setup.TileWorldSize;

            chunkObject.transform.localPosition = new Vector3(
                (chunk.Coordinate.x + 0.5f) * chunkWorldSize,
                (chunk.Coordinate.y + 0.5f) * chunkWorldSize,
                0f);
        }

        /// <summary>
        /// 실행 상태에 맞는 방식으로 생성 오브젝트를 제거합니다.
        /// </summary>
        private static void DestroyGeneratedObject(Object generatedObject)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedObject);
                return;
            }

            DestroyImmediate(generatedObject);
        }
    }
}