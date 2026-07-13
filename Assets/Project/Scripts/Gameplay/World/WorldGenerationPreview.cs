using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 여러 청크를 생성하여 연결 상태를 미리 확인합니다.
    /// </summary>
    public sealed class WorldGenerationPreview : MonoBehaviour
    {
        [SerializeField] private int seed = 12345;

        [SerializeField] private Vector2Int previewSize = new Vector2Int(3, 3);

        [SerializeField] private float pixelsPerUnit = 32f;

        private readonly List<GameObject> chunkObjects = new();


        private void Start()
        {
            Generate();
        }

        private void OnDestroy()
        {
            chunkObjects.Clear();
        }

        /// <summary>
        /// 설정된 범위의 청크를 생성합니다.
        /// </summary>
        public void Generate()
        {
            Clear();

            CaveChunkGenerator generator = new CaveChunkGenerator(seed);

            int startX = -(previewSize.x / 2);
            int startY = -(previewSize.y / 2);

            for (int y = 0; y < previewSize.y; y++)
            {
                for (int x = 0; x < previewSize.x; x++)
                {
                    Vector2Int chunkCoordinate = new Vector2Int(startX + x, startY + y);

                    CreateChunk(generator, chunkCoordinate);
                }
            }
        }

        /// <summary>
        /// 단일 청크 오브젝트를 생성하고 월드 위치에 배치합니다.
        /// </summary>
        private void CreateChunk(CaveChunkGenerator generator, Vector2Int chunkCoordinate)
        {
            WorldChunk chunk = generator.Generate(chunkCoordinate);

            GameObject chunkObject = new GameObject(
                $"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}");

            chunkObject.transform.SetParent(transform, false);

            WorldChunkRenderer chunkRenderer =
                chunkObject.AddComponent<WorldChunkRenderer>();

            chunkRenderer.PixelsPerUnit = pixelsPerUnit;
            chunkRenderer.Render(chunk);

            float chunkWorldSize = WorldChunk.Size / pixelsPerUnit;

            chunkObject.transform.localPosition = new Vector3(
                (chunkCoordinate.x + 0.5f) * chunkWorldSize,
                (chunkCoordinate.y + 0.5f) * chunkWorldSize,
                0f);

            chunkObjects.Add(chunkObject);
        }

        /// <summary>
        /// 이전에 생성한 미리보기 청크를 제거합니다.
        /// </summary>
        private void Clear()
        {
            foreach (GameObject chunkObject in chunkObjects)
            {
                if (chunkObject != null)
                {
                    Destroy(chunkObject);
                }
            }

            chunkObjects.Clear();
        }
    }
}