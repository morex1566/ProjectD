using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    public class WorldCameraController : MonoBehaviour
    {
        [Header(nameof(WorldCameraController) + ".Runtime")]

        [SerializeField, ReadOnly] private Camera cam;

        [SerializeField] private Ease cameraMoveEase = Ease.OutCubic;

        [SerializeField] private float cameraMoveDuration = 0.8f;

        private Tween cameraMoveTween;

        private Tween cameraZoomTween;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            WorldManager.OnMapLoaded += LookAt;
        }

        private void OnDisable()
        {
            WorldManager.OnMapLoaded -= LookAt;
        }

        /// <summary>
        /// world manager에서 맵이 로드되면 카메라가 맵의 정중앙을 바라봅니다.
        /// </summary>
        private void LookAt()
        {
            if (!WorldManager.TryGetMapCenterWorldPos(out Vector3 mapCenterWorldPos)) return;

            // 2D 카메라 깊이는 유지하고 맵 중심에 해당하는 x, y만 맞춥니다.
            Vector3 targetPos = new Vector3(mapCenterWorldPos.x, mapCenterWorldPos.y, transform.position.z);

            cameraMoveTween = transform.DOMove(targetPos, cameraMoveDuration).SetEase(cameraMoveEase);

            // 화면에 맵 전체가 보이게 할 수 있도록
            int rowCount = WorldManager.GetMapRowCount();
            cam.orthographicSize = rowCount * 0.6f;
        }

        /// <summary>
        /// 카메라가 지정된 위치를 정중앙으로 바라보게 합니다.
        /// </summary>
        public void LookAt(Vector3 targetPos)
        {
            cameraMoveTween = transform.DOMove(targetPos, cameraMoveDuration).SetEase(cameraMoveEase);
        }

        /// <summary>
        /// 2D 월드 카메라의 orthographic size를 지정 크기로 보간합니다.
        /// </summary>
        public void Zoom(float orthographicSize, float duration)
        {
            if (cam == null) return;

            cameraZoomTween?.Kill();
            cameraZoomTween = cam.DOOrthoSize(orthographicSize, duration).SetEase(cameraMoveEase);
        }
    }
}
