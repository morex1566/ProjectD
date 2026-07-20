using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public enum PlayerCommandSystemType
    {
        Idle
    }

    public enum PlayerCommandQueueMode
    {
        Append,
        Replace
    }

    /// <summary>
    /// 플레이어 명령 모드의 생명주기 계약입니다.
    /// </summary>
    public abstract class PlayerCommandSystem
    {
        public abstract void Enter();

        public abstract void Exit();
    }

    /// <summary>
    /// 기본 플레이 상태에서 Creature 선택과 이동 명령 입력을 관리합니다.
    /// </summary>
    public sealed class IdleCommandSystem : PlayerCommandSystem
    {
        private readonly CreatureSelector creatureSelector = null;

        private const int MaximumDestinationSearchDistance = 8;


        public IdleCommandSystem(CreatureSelector creatureSelector)
        {
            this.creatureSelector = creatureSelector;
        }


        /// <summary>
        /// Creature 선택과 우클릭 이동 명령 입력을 활성화합니다.
        /// </summary>
        public override void Enter()
        {
            if (creatureSelector != null)
            {
                creatureSelector.enabled = true;
            }

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.RightClick.performed += OnRightClickPerformed;
            }
        }

        /// <summary>
        /// 우클릭 이동 명령 입력과 Creature 선택을 비활성화합니다.
        /// </summary>
        public override void Exit()
        {
            if (creatureSelector != null)
            {
                creatureSelector.enabled = false;
            }

            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.RightClick.performed -= OnRightClickPerformed;
            }
        }

        /// <summary>
        /// 현재 선택된 모든 Creature의 기존 명령을 새 이동 명령으로 교체합니다.
        /// </summary>
        public void CommandMove(Vector2Int requestedCoordinate)
        {
            if (creatureSelector == null)
            {
                return;
            }

            if (WorldManager.TryGetWorldMap(out WorldMap worldMap) == false)
            {
                return;
            }

            if (WorldPathfinder.TryFindNearestStandableCoordinate(
                worldMap,
                requestedCoordinate,
                MaximumDestinationSearchDistance,
                out Vector2Int targetCoordinate) == false)
            {
                return;
            }

            foreach (CreatureController creature in creatureSelector.Selecteds)
            {
                if (creature == null || creature.isActiveAndEnabled == false)
                {
                    continue;
                }

                creature.EnqueueJob(
                    new CreatureMoveJob(creature, targetCoordinate),
                    PlayerCommandQueueMode.Replace);
            }
        }

        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            if (Pointer.current == null)
            {
                return;
            }

            Vector2 pointerScreenPosition = Pointer.current.position.ReadValue();
            if (ScreenEx.IsPointerOverUI(pointerScreenPosition) == true)
            {
                return;
            }

            WorldCameraController cameraController = WorldManager.GetWorldCameraController();
            if (cameraController == null || cameraController.Cam == null)
            {
                return;
            }

            if (MouseEx.TryGetMouseWorldPosition(cameraController.Cam, out Vector3 worldPosition) == false)
            {
                return;
            }

            Vector2Int requestedCoordinate = WorldManager.WorldToTileCoordinate(worldPosition);
            CommandMove(requestedCoordinate);
        }
    }
}
