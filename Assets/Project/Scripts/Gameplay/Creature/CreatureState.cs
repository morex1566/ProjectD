using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace TRPG.Runtime
{
    [Flags]
    public enum CreatureStateType
    {
        None = 0,
        Idle = 1 << 0,
        Move = 1 << 1,
        Dead = 1 << 2,
    }

    /// <summary>
    /// 작업을 제외한 상시 돌아야 하는 동작을 여기서 정의
    /// </summary>
    public abstract class CreatureState 
    {
        protected CreatureController controller;

        public CreatureState(CreatureController controller)
        {
            this.controller = controller;
        }

        public virtual void Update()
        {
            MoveByGravity();
        }

        public virtual void DrawGizmos()
        {

        }

        private void MoveByGravity()
        {
            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return;
            }

            float maxFallDistance = gridContext.Grid.cellSize.y * 0.5f;
            Vector3 gravity = WorldTile.DefaultGravity * Time.deltaTime;

            if (Mathf.Abs(gravity.y) > maxFallDistance)
            {
                gravity.y = -maxFallDistance;
            }

            Vector3 nextFootWorldPos = controller.GroundChecker.transform.position + gravity;

            if (gridContext.TryGetTile(WorldTilemapType.WorldTilemapGround, nextFootWorldPos, out _) == true)
            {
                Vector3Int groundCellPos = gridContext.Grid.WorldToCell(nextFootWorldPos);
                Vector3 groundCellCenterWorld = gridContext.Grid.GetCellCenterWorld(groundCellPos);

                float groundTopY = groundCellCenterWorld.y + gridContext.Grid.cellSize.y * 0.5f;
                float snapDeltaY = groundTopY - controller.GroundChecker.transform.position.y;

                controller.transform.position += Vector3.up * snapDeltaY;
                return;
            }

            controller.transform.position += gravity;
        }
    }

    public class IdleState : CreatureState
    {
        public IdleState(CreatureController controller) : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
        }
    }

    public class MoveState : CreatureState
    {
        public MoveState(CreatureController controller) : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
        }
    }

    public class DeadState : CreatureState
    {
        public DeadState(CreatureController controller) : base(controller)
        {
        }
    }
}
