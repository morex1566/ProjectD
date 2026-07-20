using MBT;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace TRPG.Runtime
{
    public partial class CreatureController
    {
        [SerializeField, Min(1)] private int maximumJumpHorizontalDistance = 2;

        [SerializeField, Min(0)] private int maximumJumpHeight = 4;

        [SerializeField, Min(0)] private int maximumFallDistance = 4;

        /// <summary>
        /// 한 칸짜리 Creature가 사용할 플랫폼 길찾기 능력을 생성합니다.
        /// </summary>
        public WorldPathMovementProfile CreatePathMovementProfile()
        {
            const int navigationBodyHeight = 1;

            return new WorldPathMovementProfile(
                navigationBodyHeight,
                maximumJumpHorizontalDistance,
                maximumJumpHeight,
                maximumFallDistance);
        }

        /// <summary>
        /// 지정된 점프 진행률에 해당하는 위치로 이동하고 완료 여부를 반환합니다.
        /// </summary>
        public bool Jump(Vector2Int startCoordinate, Vector2Int targetCoordinate, float ratio)
        {
            Vector2 jumpTilePosition = CalculateJumpPosition(startCoordinate, targetCoordinate, ratio);
            Vector2 jumpWorldPosition = WorldManager.TileToWorldPosition(jumpTilePosition);

            transform.position = jumpWorldPosition;

            return ratio >= 1f;
        }

        /// <summary>
        /// 현재 위치에서 Walk 행동의 도착 셀까지 Creature를 이동시킵니다.
        /// </summary>
        public bool Walk(WorldPathAction action)
        {
            Vector3 currentWorldPosition = transform.position;
            Vector3 targetWorldPosition = WorldManager.TileToWorldPosition(action.To);

            // MoveSpeed는 초당 타일 수로 사용합니다.
            float movementDistance = context.MoveSpeed * Time.deltaTime;

            // 이동
            Vector3 nextWorldPosition = Vector3.MoveTowards(currentWorldPosition, targetWorldPosition, movementDistance);
            transform.position = nextWorldPosition;

            if (Vector2.Distance(transform.position, targetWorldPosition) > 0.001f)
            {
                return false;
            }

            // 마지막 프레임의 오차를 제거합니다.
            transform.position = targetWorldPosition;
            return true;
        }

        /// <summary>
        /// 지정된 낙하 진행률에 해당하는 위치로 이동하고 완료 여부를 반환합니다.
        /// </summary>
        public bool Fall(WorldPathAction action, float ratio)
        {
            Vector2 fallTilePosition = CalculateFallPosition(action, ratio);
            Vector2 fallWorldPosition = WorldManager.TileToWorldPosition(fallTilePosition);

            transform.position = new Vector3(fallWorldPosition.x, fallWorldPosition.y, transform.position.z);

            return ratio >= 1f;
        }

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
        /// 낙하 경로의 현재 타일 단위 위치를 계산합니다.
        /// </summary>
        public static Vector2 CalculateFallPosition(WorldPathAction action, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            Vector2 startCoordinate = action.From;
            Vector2 entryCoordinate = GetFallEntryCoordinate(action);
            Vector2 targetCoordinate = action.To;

            float entryDistance = Vector2.Distance(startCoordinate, entryCoordinate);
            float fallDistance = Vector2.Distance(entryCoordinate, targetCoordinate);
            float totalDistance = entryDistance + fallDistance;
            float currentDistance = totalDistance * ratio;

            if (currentDistance <= entryDistance)
            {
                float entryRatio = entryDistance > 0f ? currentDistance / entryDistance : 1f;
                return Vector2.Lerp(startCoordinate, entryCoordinate, entryRatio);
            }

            float fallRatio = fallDistance > 0f ? (currentDistance - entryDistance) / fallDistance : 1f;
            return Vector2.Lerp(entryCoordinate, targetCoordinate, fallRatio);
        }

        /// <summary>
        /// 낙하 행동에서 발판 밖으로 한 칸 이동한 진입 좌표를 반환합니다.
        /// </summary>
        public static Vector2Int GetFallEntryCoordinate(WorldPathAction action)
        {
            return new Vector2Int(action.To.x, action.From.y);
        }
    }
}
