using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 카메라 이동과 PixelPerfectCamera 기반 줌을 처리합니다.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(PixelPerfectCamera))]
    public class WorldCameraController : MonoBehaviour
    {
        [ReadOnly] public Camera Cam;
        [ReadOnly] public PixelPerfectCamera PixelPerfectCam;

        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private int minAssetsPPU = 32;
        [SerializeField] private int maxAssetsPPU = 64;
        [SerializeField] private int zoomStep = 1;
        [SerializeField] private float moveSmoothTime = 0.12f;
        [SerializeField] private float zoomSmoothTime = 0.08f;

        private Vector2 moveInput;
        private Vector3 targetPosition;
        private Vector3 moveVelocity;
        private float targetAssetsPPU;
        private float currentAssetsPPU;
        private float zoomVelocity;


        /// <summary>
        /// Camera와 PixelPerfectCamera 컴포넌트를 캐싱하고 보간 목표값을 초기화합니다.
        /// </summary>
        private void Awake()
        {
            Cam = GetComponent<Camera>();
            PixelPerfectCam = GetComponent<PixelPerfectCamera>();

            targetPosition = transform.position;
            targetAssetsPPU = PixelPerfectCam.assetsPPU;
            currentAssetsPPU = PixelPerfectCam.assetsPPU;
        }

        /// <summary>
        /// 이동과 줌 입력 이벤트를 카메라 제어 함수에 연결합니다.
        /// </summary>
        private void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == false)
            {
                return;
            }

            inputMappingContext.Player.Move.performed += OnWASD;
            inputMappingContext.Player.Move.canceled += OnWASD;
            inputMappingContext.Player.ScrollWheel.performed += OnScrollWheel;
        }

        /// <summary>
        /// 비활성화 시 카메라 입력 이벤트 연결을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == false)
            {
                return;
            }

            inputMappingContext.Player.Move.performed -= OnWASD;
            inputMappingContext.Player.Move.canceled -= OnWASD;
            inputMappingContext.Player.ScrollWheel.performed -= OnScrollWheel;
        }

        /// <summary>
        /// 입력 목표 위치를 갱신하고 이동/줌 보간을 적용합니다.
        /// </summary>
        private void Update()
        {
            UpdateMoveTarget();
            SmoothMove();
            SmoothZoom();
        }

        /// <summary>
        /// 현재 이동 입력을 누적해 카메라가 따라갈 목표 위치를 갱신합니다.
        /// </summary>
        private void UpdateMoveTarget()
        {
            if (moveInput == Vector2.zero) return;

            // 대각선 입력이 직선 입력보다 빨라지지 않게 이동량을 1로 제한합니다.
            Vector2 normalizedMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
            Vector3 moveDelta = new Vector3(normalizedMoveInput.x, normalizedMoveInput.y, 0f) * moveSpeed * Time.deltaTime;
            targetPosition += moveDelta;
        }

        /// <summary>
        /// 현재 위치를 목표 위치로 부드럽게 이동시킵니다.
        /// </summary>
        private void SmoothMove()
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, moveSmoothTime);
        }

        /// <summary>
        /// 목표 assetsPPU를 향해 PixelPerfectCamera 줌을 부드럽게 보간합니다.
        /// </summary>
        private void SmoothZoom()
        {
            currentAssetsPPU = Mathf.SmoothDamp(currentAssetsPPU, targetAssetsPPU, ref zoomVelocity, zoomSmoothTime);

            // PixelPerfectCamera의 assetsPPU는 정수라 보간값을 가장 가까운 PPU로 반영합니다.
            int nextAssetsPPU = Mathf.Clamp(Mathf.RoundToInt(currentAssetsPPU), minAssetsPPU, maxAssetsPPU);
            if (PixelPerfectCam.assetsPPU != nextAssetsPPU)
            {
                PixelPerfectCam.assetsPPU = nextAssetsPPU;
            }
        }

        /// <summary>
        /// WASD 이동 입력 값을 저장합니다.
        /// </summary>
        private void OnWASD(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// 스크롤 방향에 따라 목표 줌 PPU를 한 단계씩 조절합니다.
        /// </summary>
        private void OnScrollWheel(InputAction.CallbackContext context)
        {
            Vector2 scrollDelta = context.ReadValue<Vector2>();
            if (scrollDelta.y == 0f) return;

            // assetsPPU가 커질수록 월드가 더 크게 보입니다.
            int zoomDirection = scrollDelta.y > 0f ? 1 : -1;
            targetAssetsPPU = Mathf.Clamp(targetAssetsPPU + zoomDirection * zoomStep, minAssetsPPU, maxAssetsPPU);
        }
    }
}
