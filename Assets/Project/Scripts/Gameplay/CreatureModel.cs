using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 크리처의 런타임 위치와 전투 스탯을 보관하는 모델 컴포넌트입니다.
    /// </summary>
    public class CreatureModel : MonoBehaviour
    {
        [Header(nameof(CreatureModel) + ".Setup")]

        [SerializeField] protected CreatureData data;

        [SerializeField] protected CreatureMotionSettingsData battleMotionSettings = null;

        [Header(nameof(CreatureModel) + ".Runtime")]

        [SerializeField, ReadOnly] private Vector3Int cellPos;

        [SerializeField, ReadOnly] private int hp;

        [SerializeField, ReadOnly] private int damage;

        [SerializeField, ReadOnly] private int armor;

        [SerializeField, ReadOnly] private int cost;

        [SerializeField, ReadOnly] private bool isMoveRepeatable;

        [SerializeField, ReadOnly] private List<Vector3Int> directions;


        public Vector3Int CellPos => cellPos;

        public Vector3 CellWorldPos => WorldManager.TryGetMapWorldPos(cellPos, out Vector3 cellWorldPos)
                                    ? cellWorldPos : default;
        public int Damage => damage;

        public int Hp => hp;

        public int Armor => armor;

        public int Cost => cost;    

        public bool IsMoveRepeatable => isMoveRepeatable;

        public List<Vector3Int> Directions => directions;
        
        public CreatureMotionSettingsData BattleMotionSettings => battleMotionSettings;



        /// <summary>
        /// 크리처 데이터를 주입하고 런타임 값을 초기화합니다.
        /// </summary>
        public virtual void Init(Vector3Int initCellPos, CreatureData initData = null)
        {
            data = initData;
            cellPos = initCellPos;

            hp = initData.Hp;
            damage = initData.Damage;
            armor = initData.Armor;
            isMoveRepeatable = initData.MoveRangeData.IsRepeatable;
            directions = initData.MoveRangeData.Directions;
        }

        public void SetHp(int hp)
        {
            this.hp = hp;
        }

        public void SetCellPos(Vector3Int cellPos)
        {
            this.cellPos = cellPos;
        }
    }
}
