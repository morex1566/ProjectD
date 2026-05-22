using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public partial class PlayerController : CreatureController
    {
        [System.Flags]
        private enum ActionFlag
        {
            None = 0,
            Moving = 1 << 0
        }

        [Header("PlayerController")]
        [SerializeField, ReadOnly] private DragPendulum2D dragPendulum;

        private const int DefaultMoveRange = 1;

        private ActionFlag actionFlags;

        public new PlayerModel Model => base.Model as PlayerModel;

        protected override void Awake()
        {
            base.Awake();

            dragPendulum = GetComponent<DragPendulum2D>();
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnClickCanceled;

            actionFlags = ActionFlag.None;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnClickCanceled;

            dragPendulum.EndDrag();

            WorldManager.GetInstance().ClearMoveRange();

            actionFlags = ActionFlag.None;
        }

        /// <summary>
        /// 화면 좌표가 Tilemap 레이어의 유효한 셀이면 해당 셀로 이동합니다.
        /// </summary>
        public void Move(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            if (HasActionFlag(ActionFlag.Moving)) return;

            actionFlags |= ActionFlag.Moving;
            StartCoroutine(MovementCo(targetWorldPos, targetCellPos));
        }

        private IEnumerator MovementCo(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            Vector3 startWorldPos = transform.position;
            targetWorldPos.z = transform.position.z;

            // 플레이어 전진
            float elapsedTime = 0f;
            while (elapsedTime < Model.Data.MoveDelay)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / Model.Data.MoveDelay);
                transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, progress);

                yield return null;
            }

            // 플레이어 전진 끝, 나머지 설정 후처리
            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        private bool HasActionFlag(ActionFlag flag)
        {
            return (actionFlags & flag) != ActionFlag.None;
        }

        private int GetMoveRange()
        {
            return GetMoveRange(this);
        }

        private int GetMoveRange(CreatureController creatureController)
        {
            if (creatureController.Model.MoveRange > 0) return creatureController.Model.MoveRange;

            // 스킬 데이터 에셋이 아직 연결되지 않은 상태에서도 이동 범위를 확인할 수 있게 합니다.
            return DefaultMoveRange;
        }

        private bool TryMoveToHighlightedCell(Vector3 worldPos)
        {
            if (!WorldManager.GetInstance().TryGetGroundCellPosition(worldPos, out Vector3Int cellPos)) return false;

            if (!WorldManager.GetInstance().IsMoveRangeOwner(this)) return false;

            if (!WorldManager.GetInstance().IsMovableHighlighted(cellPos)) return false;

            if (!WorldManager.GetInstance().TryGetGroundWorldPosition(cellPos, out Vector3 targetWorldPos)) return false;

            if (WorldManager.GetInstance().HasMonster(cellPos, out _)) return false;

            WorldManager.GetInstance().ClearMoveRange();

            Move(targetWorldPos, cellPos);

            return true;
        }
    }

    /// <summary>
    /// 입력
    /// </summary>
    public partial class PlayerController
    {
        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 화면좌표 구할 수 없음?
            if (!MouseEx.TryGetMouseWorldPosition(Camera.main, out Vector3 worldPos)) return;

            // 플레이어를 클릭하면 아군 이동 가능 타일을 표시합니다.
            if (Contains(worldPos))
            {
                WorldManager.GetInstance().ShowMoveRange(this, GetMoveRange(), WorldManager.GetInstance().AllyMovableTilePb);
                dragPendulum.BeginDrag(worldPos);
                return;
            }

            // 적을 클릭하면 적의 이동 가능 범위만 표시합니다.
            if (WorldManager.GetInstance().HasMonsterAtWorld(worldPos, out MonsterController monsterController))
            {
                WorldManager.GetInstance().ShowMoveRange(monsterController, GetMoveRange(monsterController), WorldManager.GetInstance().EnemyMovableTilePb);
                return;
            }

            if (TryMoveToHighlightedCell(worldPos)) return;
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            dragPendulum.EndDrag();
        }
    }
}
