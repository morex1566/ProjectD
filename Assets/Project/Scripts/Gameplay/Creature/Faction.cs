using System.Collections.Generic;

namespace TRPG.Runtime
{
    ///<summary>
    /// CreatureContext의 소속 진영 타입입니다.
    ///</summary>
    public enum FactionType
    {
        None = 0,
        Human = 1,
        Monster = 2
    }

    ///<summary>
    /// 진영 간 관계 타입입니다.
    ///</summary>
    public enum RelationType
    {
        Hostile = -1,
        Neutral = 0,
        Friendly = 1
    }

    ///<summary>
    /// 진영 간 관계를 관리하는 클래스입니다.
    ///</summary>
    public static class Faction
    {
        ///<summary>
        /// 진영 간 관계 점수 테이블입니다.
        ///</summary>
        public static readonly IReadOnlyDictionary<(FactionType from, FactionType to), int> Relations = new Dictionary<(FactionType from, FactionType to), int>
        {
            { (FactionType.None, FactionType.None), 0 },
            { (FactionType.None, FactionType.Human), 0 },
            { (FactionType.None, FactionType.Monster), 0 },

            { (FactionType.Human, FactionType.None), 0 },
            { (FactionType.Human, FactionType.Human), 100 },
            { (FactionType.Human, FactionType.Monster), -100 },

            { (FactionType.Monster, FactionType.None), 0 },
            { (FactionType.Monster, FactionType.Human), -100 },
            { (FactionType.Monster, FactionType.Monster), 100 },
        };

        ///<summary>
        /// 두 진영의 관계 점수를 반환합니다.
        ///</summary>
        public static int GetRelationValue(FactionType from, FactionType to)
        {
            // 등록된 관계가 있으면 해당 관계 점수를 반환합니다.
            if (Relations.TryGetValue((from, to), out int relationValue))
            {
                return relationValue;
            }

            // 등록되지 않은 관계는 중립 점수로 처리합니다.
            return 0;
        }

        ///<summary>
        /// 두 진영의 관계 타입을 반환합니다.
        ///</summary>
        public static RelationType GetRelationType(FactionType from, FactionType to)
        {
            // 두 진영의 관계 점수를 가져옵니다.
            int relationValue = GetRelationValue(from, to);

            if (relationValue < -50)
            {
                return RelationType.Hostile;
            }

            if (relationValue > 50)
            {
                return RelationType.Friendly;
            }

            // 관계 점수가 0이면 중립 관계입니다.
            return RelationType.Neutral;
        }

        ///<summary>
        /// 두 진영이 적대 관계인지 반환합니다.
        ///</summary>
        public static bool IsHostile(FactionType from, FactionType to)
        {
            // 관계 타입이 Hostile이면 적대 관계입니다.
            return GetRelationType(from, to) == RelationType.Hostile;
        }

        ///<summary>
        /// 두 진영이 중립 관계인지 반환합니다.
        ///</summary>
        public static bool IsNeutral(FactionType from, FactionType to)
        {
            // 관계 타입이 Neutral이면 중립 관계입니다.
            return GetRelationType(from, to) == RelationType.Neutral;
        }

        ///<summary>
        /// 두 진영이 우호 관계인지 반환합니다.
        ///</summary>
        public static bool IsFriendly(FactionType from, FactionType to)
        {
            // 관계 타입이 Friendly이면 우호 관계입니다.
            return GetRelationType(from, to) == RelationType.Friendly;
        }

        ///<summary>
        /// 두 진영이 공격 가능한 관계인지 반환합니다.
        ///</summary>
        public static bool CanAttack(FactionType from, FactionType to)
        {
            // 적대 관계일 때만 공격 가능합니다.
            return GetRelationType(from, to) == RelationType.Hostile;
        }
    }
}