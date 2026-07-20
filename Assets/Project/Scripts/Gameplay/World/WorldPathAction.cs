using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플랫폼 경로에서 실행할 행동 종류입니다.
    /// </summary>
    public enum WorldPathActionType
    {
        Walk,
        Jump,
        Fall,
    }

    /// <summary>
    /// 하나의 출발 월드 위치에서 도착 월드 위치까지 실행할 행동입니다.
    /// </summary>
    public readonly struct WorldPathAction
    {
        public WorldPathActionType Type { get; }

        public Vector2 From { get; }

        public Vector2 To { get; }

        public int Cost { get; }


        public WorldPathAction(WorldPathActionType type, Vector2 from, Vector2 to, int cost)
        {
            Type = type;
            From = from;
            To = to;
            Cost = cost;
        }

        /// <summary>
        /// 첫 행동이 Creature의 실제 위치에서 시작하도록 출발 위치만 교체합니다.
        /// </summary>
        public WorldPathAction WithFrom(Vector2 from)
        {
            return new WorldPathAction(Type, from, To, Cost);
        }
    }
}
