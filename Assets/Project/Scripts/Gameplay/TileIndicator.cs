using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    public class TileIndicator : MonoBehaviour
    {
        [Header(nameof(TileIndicator))]

        [SerializeField, ReadOnly] private CreatureController owner = null;

        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField, ReadOnly] private Vector3Int cellPos = Vector3Int.zero;


        public CreatureController Owner => owner;


        /// <summary>
        /// owner 기준 삭제를 위해 생성 시점의 소유자와 셀 좌표를 기록합니다.
        /// </summary>
        public void Init(CreatureController owner, Vector3Int cellPos)
        {
            this.owner = owner;
            this.cellPos = cellPos;
        }

        private void Awake()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// 처음 인스턴싱 되었을 때 이동가능한 부분을 알파값 100 정도로 보여줌
        /// </summary>
        public void PlayMovable()
        {

        }

        /// <summary>
        /// 이동가능한 부분이면 알파값을 255로 진하게 해서 보여줌
        /// </summary>
        public void PlayMoveTo()
        {

        }

        /// <summary>
        /// 삭제되고있는 것을 알파값을 줄여가면서 보여줌
        /// </summary>
        public void PlayDestroy()
        {
            
        }
    }
}
