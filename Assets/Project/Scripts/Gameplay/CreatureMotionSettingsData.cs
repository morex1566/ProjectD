using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 모든 크리처가 공유하는 전투 충돌 연출 타이밍 설정입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_CreatureBattleMotionSettings", menuName = "TRPG/Gameplay/Creature Battle Motion Settings")]
    public class CreatureMotionSettingsData : ScriptableObject
    {
        public const float DefaultMoveDelay = 0.02f;
        public const float DefaultStompDelay = 0.03f;
        public const float DefaultCollideDelay = 0.08f;
        public const float DefaultBattleMoveDelay = 0.12f;
        public const float DefaultBattleStompDelay = 0.04f;

        [SerializeField, Min(0f)] private float moveDelay = DefaultMoveDelay;

        [SerializeField, Min(0f)] private float stompDelay = DefaultStompDelay;

        [SerializeField, Min(0f)] private float collideDelay = DefaultCollideDelay;

        [SerializeField, Min(0f)] private float battleMoveDelay = DefaultBattleMoveDelay;

        [SerializeField, Min(0f)] private float battleStompDelay = DefaultBattleStompDelay;

        public float MoveDelay => Mathf.Max(0f, moveDelay);

        public float StompDelay => Mathf.Max(0f, stompDelay);

        public float CollideDelay => Mathf.Max(0f, collideDelay);

        public float BattleMoveDelay => Mathf.Max(0f, battleMoveDelay);

        public float BattleStompDelay => Mathf.Max(0f, battleStompDelay);
    }
}
