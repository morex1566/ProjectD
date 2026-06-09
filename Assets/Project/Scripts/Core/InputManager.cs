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

        /// <summary>
        /// 입력 매핑 컨텍스트를 생성하고 활성화합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            InputMappingContext = new InputMappingContext();
            InputMappingContext.Enable();
        }
    }
}
