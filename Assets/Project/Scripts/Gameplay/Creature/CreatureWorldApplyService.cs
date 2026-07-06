using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드에서 크리쳐에게 상시 적용시키는 동작들 (ex. gravity)
    /// </summary>
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Service - CreatureWorldApplyService")]
    public class CreatureWorldApplyService : Service
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
        }

        public override void Task()
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
}
