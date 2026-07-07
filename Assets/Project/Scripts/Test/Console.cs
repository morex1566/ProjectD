using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TRPG.Runtime
{
    /// <summary>
    /// 테스트용 콘솔 입력 이벤트를 처리합니다.
    /// </summary>
    public class Console : MonoBehaviour
    {
        [Header(nameof(IsNumberOnePressed))]

        [FormerlySerializedAs("creaturePf")]
        [SerializeField] private GameObject creaturePrefab;

        private void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Test.Num1Pressed.performed += IsNumberOnePressed;
            }
        }

        private void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Test.Num1Pressed.performed -= IsNumberOnePressed;
            }
        }

        private void SpawnCreature()
        {
            if (creaturePrefab == null)
            {
                return;
            }

            Camera cam = WorldManager.GetWorldCameraController().Cam;
            Vector3 mouseWorldPosition = MouseEx.GetMouseWorldPosition(cam);
            WorldManager.SpawnCreature(creaturePrefab, mouseWorldPosition);
        }

        /// <summary>
        /// 키보드 상단 1 또는 넘패드 1 입력 여부를 반환합니다.
        /// </summary>
        private void IsNumberOnePressed(InputAction.CallbackContext context)
        {
            SpawnCreature();
        }
    }
}
