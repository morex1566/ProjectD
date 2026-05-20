using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public class CreatureModel : MonoBehaviour, ISelectable
    {
        [Header("Runtime")]

        [SerializeField, ReadOnly] private Vector3Int cellPos;

        [SerializeField, ReadOnly] private int moveRange;

        [SerializeField, ReadOnly] private float damage;

        [SerializeField, ReadOnly] private float hp;

        [Header("Setup")]

        [SerializeField] private SpriteRenderer spriteRenderer = null;

        [SerializeField] protected CreatureData data;

        [SerializeField] private SkillData skillData;



        public bool CanSelect { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public SkillData SkillData => skillData;

        public Vector3Int CellPos => cellPos;

        public float Damage => damage;

        public float Hp => hp;



        private void OnValidate()
        {
            Init();
        }

        protected virtual void Awake()
        {
            Init();
        }

        private void Init()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            SetSkillData(skillData);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        /// <summary>
        /// 좌표가 이 크리처의 렌더러 영역 안에 있는지 검사합니다.
        /// </summary>
        public bool Contains(Vector3 position)
        {
            position.z = spriteRenderer.bounds.center.z;

            return spriteRenderer.bounds.Contains(position);
        }

        /// <summary>
        /// 현재 선택 상태를 저장합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// 현재 Transform 위치를 지정된 Tilemap의 셀 좌표로 변환합니다.
        /// </summary>
        public Vector3Int GetCurrentTilePos(Tilemap tilemap)
        {
            return tilemap.WorldToCell(transform.position);
        }

        /// <summary>
        /// 스킬 데이터를 설정하고, 해당 데이터와 관련된 런타임 스텟 초기화합니다.
        /// </summary>
        /// <param name="skillData"></param>
        public void SetSkillData(SkillData skillData)
        {
            this.skillData = skillData;

            moveRange = SkillData.moveRange;
            damage = SkillData.damage;
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
