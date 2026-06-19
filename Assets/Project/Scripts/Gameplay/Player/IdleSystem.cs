using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class IdleSystem : PlayerCommandSystem
    {
        public override void HandleRightClickPerformed()
        {
            if (!TryGetMouseCellPos(out Vector3Int targetCellPos)) return;

            AStarMoveCommand command = new AStarMoveCommand(targetCellPos, PlayerCommandEnqueueType.Replace);

            ApplyCommandToSelectedCreatures(command);
        }

        private bool TryGetMouseCellPos(out Vector3Int targetCellPos)
        {
            targetCellPos = default;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();

            // 마우스 포인터가 UI에 있음?
            if (ScreenEx.IsPointerOverUI(pointerScreenPos))
            {
                return false;
            }

            // 마우스 좌표를 월드 좌표로 변환 가능?
            if (!MouseEx.TryGetWorldPos(WorldManager.CamController.Cam, pointerScreenPos, out Vector3 pointerWorldPos))
            {
                return false;
            }

            // TODO 아마도 타일맵이 비어있다면? 클릭이 안될지도?
            targetCellPos = WorldManager.MapController.Ground.WorldToCell(pointerWorldPos);
            return true;
        }

        private void ApplyCommandToSelectedCreatures(PlayerCommand command)
        {
            // 다음 단계에서 실제 선택 목록 접근 방식을 정리합니다.
        }
    }
}
