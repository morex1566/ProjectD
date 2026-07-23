using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// Input System 액션 맵을 생성하고 활성화하는 전역 입력 관리자입니다.
    /// </summary>
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        private InputMappingContext inputMappingContext;

        public InputMappingContext InputMappingContext => GetInstance().inputMappingContext;



        public static bool TryGetInputMappingContext(out InputMappingContext context)
        {
            context = instance != null ? instance.inputMappingContext : null;
            return context != null;
        }

        /// <summary>
        /// 입력 매핑 컨텍스트를 생성하고 활성화합니다.
        /// </summary>
        public static void Init()
        {
            InputManager manager = GetInstance();

            manager.inputMappingContext?.Disable();
            manager.inputMappingContext?.Dispose();
            manager.inputMappingContext = new InputMappingContext();
            manager.inputMappingContext.Enable();
        }

        /// <summary>
        /// InputActionAsset을 비활성화하고 내부 리소스를 해제합니다.
        /// </summary>
        protected override void OnDestroy()
        {
            inputMappingContext?.Disable();
            inputMappingContext?.Dispose();
            inputMappingContext = null;

            base.OnDestroy();
        }
    }
}
