using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


namespace TRPG.Runtime
{
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


        private void Awake()
        {
            Cam = GetComponent<Camera>();
            PixelPerfectCam = GetComponent<PixelPerfectCamera>();

            targetPosition = transform.position;
            targetAssetsPPU = PixelPerfectCam.assetsPPU;
            currentAssetsPPU = PixelPerfectCam.assetsPPU;
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.Move.performed += OnWASD;
            InputManager.InputMappingContext.Player.Move.canceled += OnWASD;
            InputManager.InputMappingContext.Player.ScrollWheel.performed += OnScrollWheel;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.Move.performed -= OnWASD;
            InputManager.InputMappingContext.Player.Move.canceled -= OnWASD;
            InputManager.InputMappingContext.Player.ScrollWheel.performed -= OnScrollWheel;
        }

        private void Update()
        {
            UpdateMoveTarget();
            SmoothMove();
            SmoothZoom();
        }

        private void UpdateMoveTarget()
        {
            if (moveInput == Vector2.zero) return;

            // 대각선 입력이 직선 입력보다 빨라지지 않게 이동량을 1로 제한합니다.
            Vector2 normalizedMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
            Vector3 moveDelta = new Vector3(normalizedMoveInput.x, normalizedMoveInput.y, 0f) * moveSpeed * Time.deltaTime;
            targetPosition += moveDelta;
        }

        private void SmoothMove()
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, moveSmoothTime);
        }

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

        private void OnWASD(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

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
