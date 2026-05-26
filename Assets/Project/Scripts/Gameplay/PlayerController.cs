using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 크리처의 입력 처리와 이동 행동을 담당합니다.
    /// </summary>
    public partial class PlayerController : CreatureController
    {
        [Header("PlayerController")]

        [SerializeField] private DragPendulum2D dragger;

        public new PlayerModel Model => base.Model as PlayerModel;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnClickCanceled;
            InputManager.InputMappingContext.Player.Point.performed += OnPointPerformed;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnClickCanceled;
            InputManager.InputMappingContext.Player.Point.performed -= OnPointPerformed;
        }

        /// <summary>
        /// 이동 범위에 있는 크리쳐에게 외곽선 효과 요청
        /// </summary>
        private void RequestShowOutline(bool active)
        {
            List<CreatureController> creatureControllers = WorldManager.GetInstance().GetCreaturesInCellPosList(movableCellPosList);
            foreach (CreatureController creatureController in creatureControllers)
            {
                creatureController.SetOutline(active);
            }
        }
    }

    /// <summary>
    /// 입력
    /// </summary>
    public partial class PlayerController
    {
        private void OnPointPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 드래깅 중이지 않고
            // 플레이어 호버링하면 외곽선 쉐이딩
            bool isOutlineEnable = !dragger.gameObject.activeSelf && Contains(mouseWorldPos);

            SetOutline(isOutlineEnable);
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            WorldManager worldManager = WorldManager.GetInstance();
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 플레이어를 클릭하면 아군 이동 가능 타일을 표시합니다.
            if (!Contains(mouseWorldPos)) return;

            // 이동할 수 없는 타일 선택?
            if (!worldManager.TryGetGroundCellPos(mouseWorldPos, out Vector3Int mouseCellPos)) return;

            // 드래깅 시작
            // 이동 범위 요청
            // 이동 범위에 있는 크리쳐에게 외곽선 효과 요청
            movableCellPosList = worldManager.GetMovableCellPos(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, true);
            var indicatorCellPosList = worldManager.GetMovableCellPos(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, false);
            worldManager.AddAllyTileIndicator(indicatorCellPosList, this);
            dragger.gameObject.SetActive(true);
            RequestShowOutline(true);
            PlayPick();
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            WorldManager worldManager = WorldManager.GetInstance();
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);
            worldManager.TryGetGroundWorldPos(Model.CellPos, out Vector3 modelWorldPos);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 드래깅 중임?
            if (!dragger.gameObject.activeSelf) return;

            // 마우스 커서가 있는 Ground 타일로 이동
            if (worldManager.TryGetGroundCellPos(mouseWorldPos, out Vector3Int mouseCellPos) &&
                worldManager.TryGetGroundWorldPos(mouseCellPos, out mouseWorldPos) &&
                movableCellPosList.Contains(mouseCellPos))
            {
                Move(mouseWorldPos, mouseCellPos, Quaternion.identity);
            }
            // 이동 실패. 원래 타일로 이동
            else
            {
                Move(modelWorldPos, Model.CellPos, Quaternion.identity);
            }

            // 드래깅 끝
            // 이동 범위 초기화
            PlayDrop();
            RequestShowOutline(false);
            dragger.gameObject.SetActive(false);
            worldManager.RemoveTileIndicators(this);
            movableCellPosList = new();
        }
    }
}
