using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// 한 프레임 동안 수집된 플레이어 입력 값을 전달하는 스냅샷입니다.
    /// </summary>
    public struct InputSnapshot
    {
        // Delta
        public Vector2 move;
        public Vector2 look;

        // Trigger
        public bool attackPressed;
        public bool rollPressed;
        public bool reloadPressed;

        public bool IsEmpty => Equals(default(InputSnapshot));

        /// <summary>
        /// 이번 프레임 입력 스냅샷을 반환하고 트리거 입력을 소비 상태로 초기화합니다.
        /// </summary>
        public InputSnapshot Consume()
        {
            InputSnapshot value = this;
            this = default;
            move = value.move;
            look = value.look;

            return value;
        }
    }

    /// <summary>
    /// Input System 액션 맵을 생성하고 활성화하는 전역 입력 관리자입니다.
    /// </summary>
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        public static InputMappingContext InputMappingContext;

        public static event Action<Vector2> LeftClickStarted;

        public static event Action<Vector2> LeftClickCanceled;

        public static event Action<Vector2> RightClickStarted;

        /// <summary>
        /// 입력 매핑 컨텍스트를 생성하고 활성화합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            if (InputMappingContext != null) return;

            {
                InputMappingContext = new InputMappingContext();
                InputMappingContext.Enable();
            }
        }

        private void Update()
        {
            if (!TryGetPointerScreenPosition(out Vector2 screenPosition)) return;

            if (IsLeftClickPressedThisFrame())
            {
                LeftClickStarted?.Invoke(screenPosition);
            }

            if (IsLeftClickReleasedThisFrame())
            {
                LeftClickCanceled?.Invoke(screenPosition);
            }

            if (IsRightClickPressedThisFrame())
            {
                RightClickStarted?.Invoke(screenPosition);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (InputMappingContext != null)
            {
                InputMappingContext.Dispose();
                InputMappingContext = null;
            }

            LeftClickStarted = null;
            LeftClickCanceled = null;
            RightClickStarted = null;
        }

        private static bool IsLeftClickPressedThisFrame()
        {
            if (InputMappingContext != null)
            {
                return InputMappingContext.Player.LeftClick.WasPressedThisFrame();
            }

            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        private static bool IsLeftClickReleasedThisFrame()
        {
            if (InputMappingContext != null)
            {
                return InputMappingContext.Player.LeftClick.WasReleasedThisFrame();
            }

            return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        }

        private static bool IsRightClickPressedThisFrame()
        {
            if (InputMappingContext != null)
            {
                return InputMappingContext.Player.RightClick.WasPressedThisFrame();
            }

            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        }

        private static bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
            screenPosition = default;

            if (Pointer.current == null) return false;

            screenPosition = Pointer.current.position.ReadValue();
            return IsFinite(screenPosition);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
