using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnClickCanceled;
        }
    }

    /// <summary>
    /// 입력
    /// </summary>
    public partial class PlayerController
    {
        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 플레이어를 클릭하면 아군 이동 가능 타일을 표시합니다.
            if (!Contains(mouseWorldPos)) return;

            // 이동할 수 없는 타일 선택?
            if (!WorldManager.GetInstance().TryGetGroundCellPos(mouseWorldPos, out Vector3Int mouseCellPos)) return;


            dragger.gameObject.SetActive(true);
            PlayPick();
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            WorldManager worldManager = WorldManager.GetInstance();
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);
            worldManager.TryGetGroundWorldPos(Model.CellPos, out Vector3 modelWorldPos);

            // 드래깅 중임?
            if (!dragger.gameObject.activeSelf) return;

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 마우스 커서가 있는 Ground 타일로 이동
            if (worldManager.TryGetGroundCellPos(mouseWorldPos, out Vector3Int mouseCellPos) &&
                worldManager.TryGetGroundWorldPos(mouseCellPos, out mouseWorldPos))
            {
                Move(mouseWorldPos, mouseCellPos, Quaternion.identity);
            }
            // 이동 실패. 원래 타일로 이동
            else
            {
                Move(modelWorldPos, Model.CellPos, Quaternion.identity);
            }

            dragger.gameObject.SetActive(false);
            PlayDrop();
        }
    }
}
