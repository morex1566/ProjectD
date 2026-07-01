using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 런타임 객체와 Unity Tilemap 표현을 연결합니다.
    /// </summary>
    [RequireComponent(typeof(WorldGridContext))]
    public class WorldGridController : MonoBehaviour
    {
        [Header(nameof(WorldGridController))]

        /// <summary>
        /// 맵 생성 시작 셀 위치입니다.
        /// </summary>
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        /// <summary>
        /// 런타임 맵 상태입니다.
        /// </summary>
        [SerializeField, ReadOnly] private WorldGridContext context = null;



        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            context = GetComponent<WorldGridContext>();
        }
    }
}
