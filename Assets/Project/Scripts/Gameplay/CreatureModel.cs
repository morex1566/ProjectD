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

        [SerializeField, ReadOnly] private float armor;

        [Header("Setup")]

        [SerializeField] protected CreatureData data;



        public Vector3Int CellPos => cellPos;

        public int MoveRange => moveRange;

        public float Damage => damage;

        public float Hp => hp;

        public float Armor => armor;



        /// <summary>
        /// 크리처 데이터를 주입하고 런타임 값을 초기화합니다.
        /// </summary>
        public virtual void Init(Vector3Int cellPos, CreatureData data = null)
        {
            this.data = data;
            this.cellPos = cellPos;
            moveRange = 0;

            if (data == null) return;

            hp = data.Hp;
            damage = data.Damage;
            armor = data.Armor;

            // 현재 선택된 기본 스킬의 범위를 실제 이동 가능 범위로 사용합니다.
            if (data.SkillData != null) moveRange = Mathf.Max(0, data.SkillData.moveRange);
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
