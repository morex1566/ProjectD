using UnityEngine;

namespace TRPG.Runtime
{
    [DisallowMultipleComponent]
    public class DragPendulum2D : MonoBehaviour
    {
        [Header("Drag")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float followSmoothTime = 0f;
        [SerializeField] private Vector2 holdOffset = new Vector2(0f, 0f);

        [Header("Rotation")]
        [SerializeField] private float rotateAmount = 30f;
        [SerializeField] private float rotateSmooth = 6f;

        private bool isDragging;
        private Vector3 velocity;
        private Vector3 previousMouseWorldPos;

        private void Awake()
        {
            ResolveTargetCamera();
        }

        private void Update()
        {
            if (!isDragging) return;
            if (!MouseEx.TryGetMouseWorldPosition(ResolveTargetCamera(), out Vector3 mouseWorldPos)) return;

            FollowMouseWithInertia(mouseWorldPos);
        }

        /// <summary>
        /// PlayerController의 입력 이벤트에서 드래그를 시작합니다.
        /// </summary>
        public void BeginDrag(Vector3 mouseWorldPos)
        {
            isDragging = true;
            velocity = Vector3.zero;
            previousMouseWorldPos = mouseWorldPos;
        }

        /// <summary>
        /// PlayerController의 입력 이벤트에서 드래그를 종료합니다.
        /// </summary>
        public void EndDrag()
        {
            isDragging = false;
            velocity = Vector3.zero;
        }

        private void FollowMouseWithInertia(Vector3 mouseWorldPos)
        {
            // 캐릭터가 마우스 아래에 대롱대롱 매달리도록 오프셋 적용
            Vector3 targetPos = mouseWorldPos + (Vector3)holdOffset;
            targetPos.z = transform.position.z;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmoothTime);

            // 마우스 이동 방향에 따라 캐릭터가 살짝 기울어짐
            Vector3 mouseDelta = mouseWorldPos - previousMouseWorldPos;
            float targetZRot = -mouseDelta.x * rotateAmount;

            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetZRot);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSmooth);

            previousMouseWorldPos = mouseWorldPos;
        }

        private Camera ResolveTargetCamera()
        {
            if (targetCamera == null) targetCamera = Camera.main;

            return targetCamera;
        }
    }
}
