using UnityEngine;

namespace TRPG.Runtime
{
    public class WorldCameraController : MonoBehaviour
    {
        [Header(nameof(WorldCameraController) + ".Runtime")]

        [SerializeField, ReadOnly] private Camera cam;

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

        private void Update()
        {

        }

        /// <summary>
        /// world manager에서 맵이 로드되면 카메라가 맵의 정중앙을 바라봅니다.
        /// </summary>
        private void LookAt()
        {
            if (!WorldManager.TryGetMapCenterWorldPos(out Vector3 mapCenterWorldPos)) return;

            // 2D 카메라 깊이는 유지하고 맵 중심에 해당하는 x, y만 맞춥니다.
            transform.position = new Vector3(mapCenterWorldPos.x, mapCenterWorldPos.y, transform.position.z);

            // 화면에 맵 전체가 보이게 할 수 있도록
            int rowCount = WorldManager.GetMapRowCount();
            cam.orthographicSize = rowCount * 0.6f;
        }
    }
}
