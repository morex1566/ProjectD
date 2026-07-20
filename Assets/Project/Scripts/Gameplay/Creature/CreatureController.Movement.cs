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
        public bool Jump(Vector2 startWorldPosition, Vector2 targetWorldPosition, float ratio)
        {
            Vector2 jumpWorldPosition = CalculateJumpPosition(startWorldPosition, targetWorldPosition, ratio);

            transform.position = new Vector3(jumpWorldPosition.x, jumpWorldPosition.y, transform.position.z);

            return ratio >= 1f;
        }

        /// <summary>
        /// 현재 위치에서 Walk 행동의 도착 셀까지 Creature를 이동시킵니다.
        /// </summary>
        public bool Walk(WorldPathAction action)
        {
            Vector3 currentWorldPosition = transform.position;
            Vector3 targetWorldPosition = new Vector3(action.To.x, action.To.y, currentWorldPosition.z);

            // MoveSpeed는 초당 타일 수로 사용합니다.
            float tileWorldSize = WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize;
            float movementDistance = context.MoveSpeed * tileWorldSize * Time.deltaTime;

            // 이동
            float nextWorldPositionX = Mathf.MoveTowards(currentWorldPosition.x, targetWorldPosition.x, movementDistance);
            transform.position = new Vector3(nextWorldPositionX, currentWorldPosition.y, currentWorldPosition.z);

            if (Mathf.Abs(transform.position.x - targetWorldPosition.x) > 0.001f)
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
            Vector2 fallWorldPosition = CalculateFallPosition(action, ratio);
            transform.position = new Vector3(fallWorldPosition.x, fallWorldPosition.y, transform.position.z);

            return ratio >= 1f;
        }

        /// <summary>
        /// 출발 월드 위치와 도착 월드 위치를 연결하는 점프 곡선의 위치를 계산합니다.
        /// </summary>
        public static Vector2 CalculateJumpPosition(Vector2 startWorldPosition, Vector2 targetWorldPosition, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            float middleHeight = (startWorldPosition.y + targetWorldPosition.y) * 0.5f;
            float apexHeight = Mathf.Max(startWorldPosition.y, targetWorldPosition.y) + WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize;
            float arcHeight = apexHeight - middleHeight;

            float horizontalPosition = Mathf.Lerp(startWorldPosition.x, targetWorldPosition.x, ratio);
            float linearVerticalPosition = Mathf.Lerp(startWorldPosition.y, targetWorldPosition.y, ratio);
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

            Vector2 startWorldPosition = action.From;
            Vector2 entryWorldPosition = GetFallEntryPosition(action);
            Vector2 targetWorldPosition = action.To;

            float entryDistance = Vector2.Distance(startWorldPosition, entryWorldPosition);
            float fallDistance = Vector2.Distance(entryWorldPosition, targetWorldPosition);
            float totalDistance = entryDistance + fallDistance;
            float currentDistance = totalDistance * ratio;

            if (currentDistance <= entryDistance)
            {
                float entryRatio = entryDistance > 0f ? currentDistance / entryDistance : 1f;
                return Vector2.Lerp(startWorldPosition, entryWorldPosition, entryRatio);
            }

            float fallRatio = fallDistance > 0f ? (currentDistance - entryDistance) / fallDistance : 1f;
            return Vector2.Lerp(entryWorldPosition, targetWorldPosition, fallRatio);
        }

        /// <summary>
        /// 낙하 행동에서 발판 밖으로 이동한 진입 월드 위치를 반환합니다.
        /// </summary>
        public static Vector2 GetFallEntryPosition(WorldPathAction action)
        {
            return new Vector2(action.To.x, action.From.y);
        }
    }
}
