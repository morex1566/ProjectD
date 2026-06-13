using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    

    public class MapController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private MapGenerator mapGenerator = null;

        [SerializeField, ReadOnly] private Dictionary<int, GameObject> loadedChunks = new();

        [SerializeField] private GameObject groundTile = null;

        private Map map = null;



        private void Awake()
        {
            mapGenerator = GetComponent<MapGenerator>();
        }

        private void Start()
        {
            map = mapGenerator.Generate();
        }

        private void Update()
        {
            UpdateChunks();
        }

        private void UpdateChunks()
        {
            // 캠이 보는곳에 청크 생성 가능?
            if (!map.TryGetChunk(WorldManager.CamController.Cam.transform.position, out Chunk chunk, out int chunkIndex)) return;

            // 이미 로드된 청크임?
            if (loadedChunks.ContainsKey(chunkIndex)) return;

            GameObject chunkObj = new GameObject($"chunk{chunkIndex}");
            chunkObj.transform.SetParent(transform, false);
            loadedChunks.Add(chunkIndex, chunkObj);

            // Scene에서 청크 인덱스 순서로 배치될 수 있도록
            UpdateSiblingOrder();

            // 청크에 속한 타일 인스턴싱
            int landTileCount = 0;

            foreach (var tile in chunk.Tiles)
            {
                if (tile.type != MapTileType.Land) continue;

                landTileCount++;
                var tileInst = Instantiate(groundTile, tile.worldPos, Quaternion.identity, chunkObj.transform);
                var spriter = tileInst.GetComponentInChildren<SpriteRenderer>();
                spriter.sortingOrder = chunkIndex;
            }

            Debug.Log($"Loaded chunk {chunkIndex}. Land tiles: {landTileCount}/{chunk.Tiles.Count}");
        }

        private void UpdateSiblingOrder()
        {
            int siblingIndex = 0;

            for (int chunkIndex = 0; chunkIndex < map.Chunks.Length; chunkIndex++)
            {
                if (!loadedChunks.TryGetValue(chunkIndex, out GameObject chunkObj)) continue;

                // 낮은 청크 인덱스가 Hierarchy 상단에 오도록 sibling 순서를 맞춥니다.
                chunkObj.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }
    }
}
