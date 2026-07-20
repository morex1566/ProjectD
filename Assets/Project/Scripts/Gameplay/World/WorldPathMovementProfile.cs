using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 기반 플랫폼 길찾기에 사용하는 크리처의 이동 능력입니다.
    /// </summary>
    public readonly struct WorldPathMovementProfile
    {
        /// <summary>
        /// 크리처가 차지하는 세로 타일 수입니다.
        /// </summary>
        public int BodyHeight { get; }

        /// <summary>
        /// 한 번의 점프로 이동할 수 있는 최대 수평 타일 수입니다.
        /// </summary>
        public int MaximumJumpHorizontalDistance { get; }

        /// <summary>
        /// 한 번의 점프로 올라갈 수 있는 최대 타일 높이입니다.
        /// </summary>
        public int MaximumJumpHeight { get; }

        /// <summary>
        /// 하나의 낙하 행동으로 내려갈 수 있는 최대 타일 거리입니다.
        /// </summary>
        public int MaximumFallDistance { get; }


        public WorldPathMovementProfile(int bodyHeight, int maximumJumpHorizontalDistance, int maximumJumpHeight, int maximumFallDistance)
        {
            BodyHeight = bodyHeight;
            MaximumJumpHorizontalDistance = maximumJumpHorizontalDistance;
            MaximumJumpHeight = maximumJumpHeight;
            MaximumFallDistance = maximumFallDistance;
        }

        /// <summary>
        /// 길찾기에 사용할 수 있는 이동 능력인지 확인합니다.
        /// </summary>
        public bool IsValid()
        {
            return BodyHeight > 0 &&
                   MaximumJumpHorizontalDistance > 0 &&
                   MaximumJumpHeight >= 0 &&
                   MaximumFallDistance >= 0;
        }
    }
}
