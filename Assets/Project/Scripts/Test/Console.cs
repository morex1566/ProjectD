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
        [FormerlySerializedAs("creaturePf")]
        [SerializeField] private GameObject creaturePrefab;

        [SerializeField] private CreatureIdData num1CreatureIdData = null;

        [SerializeField] private CreatureIdData num2CreatureIdData = null;

        private void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Test.Num1Pressed.performed += IsNumberOnePressed;
                inputMappingContext.Test.Num2Pressed.performed += IsNumberTwoPressed;
            }
        }

        private void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Test.Num1Pressed.performed -= IsNumberOnePressed;
                inputMappingContext.Test.Num2Pressed.performed -= IsNumberTwoPressed;
            }
        }

        private void SpawnCreature(CreatureIdData creatureIdData)
        {
            if (creaturePrefab == null)
            {
                return;
            }

            if (creatureIdData == null || string.IsNullOrEmpty(creatureIdData.Id) == true)
            {
                return;
            }

            Camera cam = WorldManager.GetWorldCameraController().Cam;
            Vector3 mouseWorldPosition = MouseEx.GetMouseWorldPosition(cam);
            WorldManager.SpawnCreature(creaturePrefab, creatureIdData.Id, mouseWorldPosition);
        }

        /// <summary>
        /// 키보드 1 입력 여부를 반환합니다.
        /// </summary>
        private void IsNumberOnePressed(InputAction.CallbackContext context)
        {
            SpawnCreature(num1CreatureIdData);
        }

        /// <summary>
        /// 키보드 2 입력 여부를 반환합니다.
        /// </summary>
        private void IsNumberTwoPressed(InputAction.CallbackContext context)
        {
            SpawnCreature(num2CreatureIdData);
        }
    }
}
