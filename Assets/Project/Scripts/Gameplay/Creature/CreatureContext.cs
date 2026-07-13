using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    [Serializable]
    public class CreatureContext
    {
        public float BaseAtk = 1;

        public float BaseAttackRange = 1;

        public float BaseAttackSpeed = 1;

        public float Hp = 1;

        public float Atk = 1;

        public float DetectRange = 1;

        public float AttackRange = 1;

        public float AttackSpeed = 1;

        public string Id;

        public string Name;

        public string Description;

        public FactionType Faction = FactionType.None;

        public Sprite Sprite;

        public static FactionType ParseFaction(string faction)
        {
            if (string.IsNullOrWhiteSpace(faction))
            {
                return FactionType.None;
            }

            if (Enum.TryParse(faction, true, out FactionType parsedFaction))
            {
                return parsedFaction;
            }

            Debug.LogWarning($"Invalid Creature Faction: {faction}");
            return FactionType.None;
        }
    }
}
