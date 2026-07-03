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

        private void MoveByGravity()
        {
            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return;
            }

            Vector3 gravity = WorldTile.DefaultGravity * Time.deltaTime;
            Vector3 nextGroundCheckerWorldPos = controller.GroundChecker.transform.position + gravity;
            Vector3Int nextGroundCellPos = gridContext.Grid.WorldToCell(nextGroundCheckerWorldPos);

            // 아래는 이제 이동할 수 없는 곳인가?
            if (gridContext.TryGetTile(WorldTilemapType.WorldTilemapGround, nextGroundCheckerWorldPos, out _) == true)
            {
                Vector3 groundCellCenterWorld = gridContext.Grid.GetCellCenterWorld(nextGroundCellPos);
                float groundTopY = groundCellCenterWorld.y + gridContext.Grid.cellSize.y * 0.5f;
                float snapDeltaY = controller.GroundChecker.transform.position.y - groundTopY;

                controller.transform.position += Vector3.down * snapDeltaY;
            }
            else
            {
                controller.transform.position += gravity;
            }
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
