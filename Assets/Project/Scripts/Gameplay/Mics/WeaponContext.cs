using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WeaponData를 복사해서 생성되는 런타임 장착 상태입니다.
    /// </summary>
    [Serializable]
    public class WeaponContext
    {
        public string Id;

        public string Name;

        public string Description;

        public float Damage;

        public float AttackRange;

        public float AttackSpeed;

        public float Weight;

        public Sprite Sprite;

        public void Load(WeaponData data)
        {
            if (data == null)
            {
                Clear();
                return;
            }

            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            Damage = data.Damage;
            AttackRange = data.AttackRange;
            AttackSpeed = data.AttackSpeed;
            Weight = data.Weight;
            Sprite = data.Sprite;
        }

        public void Load(WeaponContext context)
        {
            if (context == null)
            {
                Clear();
                return;
            }

            Id = context.Id;
            Name = context.Name;
            Description = context.Description;
            Damage = context.Damage;
            AttackRange = context.AttackRange;
            AttackSpeed = context.AttackSpeed;
            Weight = context.Weight;
            Sprite = context.Sprite;
        }

        public void Clear()
        {
            Id = null;
            Name = null;
            Description = null;
            Damage = 0f;
            AttackRange = 0f;
            AttackSpeed = 0f;
            Weight = 0f;
            Sprite = null;
        }
    }
}
