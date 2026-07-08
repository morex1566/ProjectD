using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureContext AI의 이동/행동 타입입니다.
    /// </summary>
    public enum CreatureAIType
    {
        None,
        Ground,
        Air
    }

    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    [Serializable]
    public class CreatureContext
    {
        public float Hp = 1;

        public float Atk = 1;

        public float DetectRange = 1;

        public float AttackRange = 1;

        public float AttackSpeed = 1;

        public float MoveSpeed = 1;

        public CreatureAIType AIType = CreatureAIType.Ground;

        public string DataId;

        public string NameKey;

        public string DescKey;

        public string Faction;

        public Sprite Sprite;

        public GameObject BehaviourTree;


        public static CreatureAIType ParseAIType(string aiType)
        {
            if (string.IsNullOrWhiteSpace(aiType))
            {
                return CreatureAIType.None;
            }

            if (Enum.TryParse(aiType, true, out CreatureAIType parsedAIType))
            {
                return parsedAIType;
            }

            Debug.LogWarning($"Invalid Creature AIType: {aiType}");
            return CreatureAIType.None;
        }
    }
}
