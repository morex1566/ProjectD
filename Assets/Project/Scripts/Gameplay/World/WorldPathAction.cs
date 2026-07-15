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
    /// 하나의 출발 타일에서 도착 타일까지 실행할 행동입니다.
    /// </summary>
    public readonly struct WorldPathAction
    {
        public WorldPathActionType Type { get; }

        public Vector2Int From { get; }

        public Vector2Int To { get; }

        public int Cost { get; }


        public WorldPathAction(WorldPathActionType type, Vector2Int from, Vector2Int to, int cost)
        {
            Type = type;
            From = from;
            To = to;
            Cost = cost;
        }
    }
}
