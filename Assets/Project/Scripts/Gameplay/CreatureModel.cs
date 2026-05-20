using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public class CreatureModel : MonoBehaviour
    {
        [Header("Runtime")]

        [SerializeField, ReadOnly] private Vector3Int cellPos;

        [SerializeField, ReadOnly] private int moveRange;

        [SerializeField, ReadOnly] private float damage;

        [SerializeField, ReadOnly] private float hp;

        [Header("Setup")]

        [SerializeField] protected CreatureData data;



        public Vector3Int CellPos => cellPos;

        public float Damage => damage;

        public float Hp => hp;


        /// <summary>
        /// 크리처 데이터를 주입하고 런타임 값을 초기화합니다.
        /// </summary>
        public virtual void Init(CreatureData data, Vector3Int cellPos)
        {
            this.data = data;
            this.cellPos = cellPos;
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        /// <summary>
        /// 현재 Transform 위치를 지정된 Tilemap의 셀 좌표로 변환합니다.
        /// </summary>
        public Vector3Int GetCurrentTilePos(Tilemap tilemap)
        {
            return tilemap.WorldToCell(transform.position);
        }

        public void SetHp(float hp)
        {
            this.hp = hp;
        }

        public void SetCellPos(Vector3Int cellPos)
        {
            this.cellPos = cellPos;
        }
    }
}
