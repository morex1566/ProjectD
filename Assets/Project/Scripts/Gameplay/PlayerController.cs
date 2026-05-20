using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(PlayerModel))]
    public class PlayerController : CreatureController
    {
        private const float MoveInputThreshold = 0.5f;

        private Vector2 moveInput;

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClick;
            InputManager.InputMappingContext.Player.Move.performed += OnMove;
            InputManager.InputMappingContext.Player.Move.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClick;
            InputManager.InputMappingContext.Player.Move.performed -= OnMove;
            InputManager.InputMappingContext.Player.Move.canceled -= OnMoveCanceled;
            moveInput = Vector2.zero;
        }

        private void Update()
        {
            UpdateMovement();
        }

        /// <summary>
        /// 클릭 입력으로 화면 좌표 아래의 타일을 검사합니다.
        /// </summary>
        private void OnClick(InputAction.CallbackContext context)
        {
            if (!MouseEx.TryGetMouseWorldPosition(Camera.main, out Vector3 screenWorldPos)) return;

            // TODO : 전투씬에서만 사용하도록 return;

            // TODO : ObjectSelector 대상으로만 한정?

            // 클릭한 위치가 이동할 수 있는 곳임?
            if (WorldManager.GetInstance().TryGetGroundCellPosition(screenWorldPos, out Vector3Int cellPos))
            {

            }

            // 공격할 수 있는곳임?
            if (WorldManager.GetInstance().HasMonster(cellPos, out MonsterController monster))
            {

            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
        }

        private void UpdateMovement()
        {
            Vector3Int cellStep = GetInputDirection(moveInput);

            Vector3Int targetCellPos = Model.CellPos + cellStep;

            // 이동 안한거임?
            if (cellStep == Vector3Int.zero) return;

            // 이미 이동중?
            if (movement != null) return;

            // 적이 있어서 공격 가능한 타일?
            if (WorldManager.GetInstance().HasMonster(targetCellPos, out MonsterController monsterController))
            {
                Attack(monsterController);
                return;
            }

            // 이동 가능한 타일?
            if (WorldManager.GetInstance().TryGetGroundWorldPosition(targetCellPos, out Vector3 targetWorldPos))
            {
                Move(targetWorldPos, targetCellPos);
                return;
            }
        }

        private Vector3Int GetInputDirection(Vector2 input)
        {
            if (input.sqrMagnitude < MoveInputThreshold * MoveInputThreshold) return Vector3Int.zero;

            // 대각 입력은 허용하지 않고 더 강한 축을 한 칸 이동 방향으로 사용합니다.
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0f ? Vector3Int.right : Vector3Int.left;
            }

            return input.y > 0f ? Vector3Int.up : Vector3Int.down;
        }
    }
}
