using UnityEngine;

namespace TRPG.Runtime
{
    public partial class CreatureController
    {
        /// <summary>
        /// 출발 타일과 도착 타일을 연결하는 점프 곡선의 위치를 계산합니다.
        /// </summary>
        public static Vector2 CalculateJumpPosition(Vector2Int startCoordinate, Vector2Int targetCoordinate, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            float middleHeight = (startCoordinate.y + targetCoordinate.y) * 0.5f;
            float apexHeight = Mathf.Max(startCoordinate.y, targetCoordinate.y) + 1f;
            float arcHeight = apexHeight - middleHeight;

            float horizontalPosition = Mathf.Lerp(startCoordinate.x, targetCoordinate.x, ratio);
            float linearVerticalPosition = Mathf.Lerp(startCoordinate.y, targetCoordinate.y, ratio);
            float arcOffset = 4f * arcHeight * ratio * (1f - ratio);
            float verticalPosition = linearVerticalPosition + arcOffset;

            return new Vector2(horizontalPosition, verticalPosition);
        }

        /// <summary>
        /// 낙하 행동에서 발판 밖으로 한 칸 이동한 진입 좌표를 반환합니다.
        /// </summary>
        public static Vector2Int GetFallEntryCoordinate(WorldPathAction action)
        {
            return new Vector2Int(action.To.x, action.From.y);
        }

        /// <summary>
        /// 타일 좌표를 Unity 월드 좌표로 변환합니다.
        /// </summary>
        public static Vector2 TileToWorld(Vector2 tilePosition, float tileWorldSize)
        {
            return tilePosition * tileWorldSize;
        }
    }
}
