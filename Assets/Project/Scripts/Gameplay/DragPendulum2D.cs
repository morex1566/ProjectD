using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 드래그 중 대상 Transform을 마우스 위치에 맞춰 이동시키고 회전 흔들림을 적용합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class DragPendulum2D : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Camera targetCamera;

        [SerializeField] private Transform targetTransform;

        [SerializeField] private Vector2 holdOffset = new Vector2(0f, 0f);

        [SerializeField] private float rotateAmount = 30f;

        [SerializeField] private float rotateSmooth = 6f;

        private Vector3 prevMouseWorldPos;



        private void Awake()
        {
            targetCamera = Camera.main;
        }

        private void Update()
        {
            FollowMouse();
        }

        /// <summary>
        /// PlayerController의 입력 이벤트에서 드래그를 시작합니다.
        /// </summary>
        private void OnEnable()
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(targetCamera);

            prevMouseWorldPos = mouseWorldPos;
        }

        private void OnDisable()
        {
            
        }

        /// <summary>
        /// 대상 Transform을 보정된 마우스 위치로 이동시키고 이동 방향에 따른 회전을 적용합니다.
        /// </summary>
        private void FollowMouse()
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(targetCamera);

            // 캐릭터가 마우스 아래에 대롱대롱 매달리도록 오프셋 적용
            Vector3 targetPos = mouseWorldPos + (Vector3)holdOffset;
            targetPos.z = targetTransform.position.z;
            targetTransform.position = targetPos;

            // 마우스 이동 방향에 따라 캐릭터가 살짝 기울어짐
            Vector3 mouseDelta = mouseWorldPos - prevMouseWorldPos;
            float targetZRot = -mouseDelta.x * rotateAmount;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetZRot);
            targetTransform.rotation = Quaternion.Lerp(targetTransform.rotation, targetRot, Time.deltaTime * rotateSmooth);

            prevMouseWorldPos = mouseWorldPos;
        }
    }
}
