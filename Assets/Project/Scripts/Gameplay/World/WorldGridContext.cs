using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    ///<summary>
    /// 런타임에서 사용하는 월드 그리드 상태와 규칙입니다.
    ///</summary>
    [ExecuteAlways]
    public class WorldGridContext : MonoBehaviour
    {
        [Header(nameof(WorldGridContext))]

        [SerializeField, ReadOnly] private Grid grid;

        ///<summary>
        /// 인스펙터에서 설정하는 Tilemap 컨텍스트 목록입니다.
        ///</summary>
        [SerializeField, ReadOnly] private List<WorldTilemapContext> tilemapContexts = new();

        ///<summary>
        /// Tilemap 타입으로 빠르게 조회하기 위한 런타임 캐시입니다.
        ///</summary>
        [SerializeField, ReadOnly] private Dictionary<WorldTilemapType, WorldTilemapContext> tilemapContextMap = new();

        ///<summary>
        /// Tilemap 컨텍스트 캐시를 반환합니다.
        ///</summary>
        public IReadOnlyDictionary<WorldTilemapType, WorldTilemapContext> TilemapContextMap => tilemapContextMap;


        public Grid Grid => grid;


        /// <summary>
        /// 타일맵 추가된거거나 사라지는 경우
        /// </summary>
        private void OnTransformChildrenChanged()
        {
            Init();
        }

        ///<summary>
        /// 인스펙터 값이 변경될 때 Tilemap 컨텍스트 캐시를 갱신합니다.
        ///</summary>
        private void OnValidate()
        {
            Init();
        }

        ///<summary>
        /// 컴포넌트가 활성화될 때 Tilemap 컨텍스트 캐시를 갱신합니다.
        ///</summary>
        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= RebuildDelayed;
#endif
        }

        private void Init()
        {
            grid = GetComponent<Grid>();

            if (Application.isPlaying)
            {
                Rebuild();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall -= RebuildDelayed;
                EditorApplication.delayCall += RebuildDelayed;
#endif
            }
        }

        [ContextMenu(nameof(CreateTilemap))]
        public void CreateTilemap()
        {
            GameObject tilemapInst = new GameObject(nameof(Tilemap));
            tilemapInst.transform.SetParent(transform);
            {
                tilemapInst.AddComponent<Tilemap>();
                tilemapInst.AddComponent<TilemapRenderer>();
                WorldTilemapContext tilemapContextComp = tilemapInst.AddComponent<WorldTilemapContext>();
                tilemapContextComp.SetOwner(this);
                tilemapContextComp.SetTilemapType(WorldTilemapType.WorldTilemapDefault);
                tilemapContextComp.Init();

                tilemapContexts.Add(tilemapContextComp);
            }

            Rebuild();
        }

        ///<summary>
        /// Tilemap 컨텍스트 조회 캐시를 다시 생성합니다.
        ///</summary>
        public void Rebuild()
        {
            tilemapContexts.RemoveAll(context => context == null);
            tilemapContextMap.Clear();

            foreach (WorldTilemapContext tilemapContext in tilemapContexts)
            {
                // 비어있는 컨텍스트는 제외합니다.
                if (tilemapContext == null)
                {
                    continue;
                }

                tilemapContext.SetOwner(this);

                // None은 실제 Tilemap 타입이 아니므로 제외합니다.
                if (tilemapContext.TilemapType == WorldTilemapType.None)
                {
                    continue;
                }

                // 중복 타입이 있으면 마지막 값으로 덮어씁니다.
                tilemapContextMap[tilemapContext.TilemapType] = tilemapContext;
            }
        }

        ///<summary>
        /// 지정한 Tilemap 레이어의 런타임 컨텍스트를 반환합니다.
        ///</summary>
        public bool TryGetTilemapContext(WorldTilemapType tilemapType, out WorldTilemapContext tilemapContext)
        {
            tilemapContext = null;

            if (tilemapType == WorldTilemapType.None)
            {
                return false;
            }

            return tilemapContextMap.TryGetValue(tilemapType, out tilemapContext) && tilemapContext != null;
        }

        ///<summary>
        /// 지정한 Tilemap 레이어에 좌표 데이터가 있는지 확인합니다.
        ///</summary>
        public bool IsInBounds(WorldTilemapType tilemapType, int x, int y)
        {
            if (TryGetTilemapContext(tilemapType, out WorldTilemapContext tilemapContext) == false)
            {
                return false;
            }

            return tilemapContext.IsInBounds(x, y);
        }

        ///<summary>
        /// Ground 레이어를 우선으로 좌표 데이터가 있는지 확인합니다.
        ///</summary>
        public bool IsInBounds(int x, int y)
        {
            return TryGetTile(x, y, out _);
        }

        public bool TryGetTile(WorldTilemapType tilemapType, Vector3 worldPos, out WorldTile tile)
        {
            tile = default;

            Vector3Int cellPos = grid.WorldToCell(worldPos);

            if (TryGetTilemapContext(tilemapType, out WorldTilemapContext tilemapContext) == false)
            {
                return false;
            }

            return tilemapContext.TryGetTile(cellPos.x, cellPos.y, out tile);
        }

        ///<summary>
        /// 지정한 Tilemap 레이어의 특정 위치 타일 데이터를 반환합니다.
        ///</summary>
        public bool TryGetTile(WorldTilemapType tilemapType, int x, int y, out WorldTile tile)
        {
            tile = default;

            if (TryGetTilemapContext(tilemapType, out WorldTilemapContext tilemapContext) == false)
            {
                return false;
            }

            return tilemapContext.TryGetTile(x, y, out tile);
        }

        ///<summary>
        /// Ground 레이어를 우선으로 특정 위치의 타일 데이터를 반환합니다.
        ///</summary>
        public bool TryGetTile(int x, int y, out WorldTile tile)
        {
            if (TryGetTile(WorldTilemapType.WorldTilemapGround, x, y, out tile))
            {
                return true;
            }

            foreach (KeyValuePair<WorldTilemapType, WorldTilemapContext> pair in tilemapContextMap)
            {
                // Ground는 이미 위에서 검사했으므로 제외합니다.
                if (pair.Key == WorldTilemapType.WorldTilemapGround)
                {
                    continue;
                }

                // 비어있는 컨텍스트는 제외합니다.
                if (pair.Value == null)
                {
                    continue;
                }

                if (pair.Value.TryGetTile(x, y, out tile))
                {
                    return true;
                }
            }

            tile = default;
            return false;
        }

#if UNITY_EDITOR
        private void RebuildDelayed()
        {
            if (this == null)
            {
                return;
            }

            Rebuild();
        }
#endif
    }
}
