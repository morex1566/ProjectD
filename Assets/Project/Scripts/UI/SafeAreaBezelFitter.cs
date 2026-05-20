using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaBezelFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform topBezel;
        [SerializeField] private RectTransform bottomBezel;
        [SerializeField] private RectTransform leftBezel;
        [SerializeField] private RectTransform rightBezel;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        /// <summary>
        /// 오브젝트가 활성화될 때 현재 Safe Area 기준으로 베젤을 배치합니다.
        /// </summary>
        private void OnEnable()
        {
            Apply();
        }

        /// <summary>
        /// 런타임 RectTransform 크기 변경 시 Safe Area 배치를 다시 계산합니다.
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (!Application.isPlaying) return;

            Apply();
        }

        /// <summary>
        /// Safe Area 또는 화면 크기가 변경되었는지 감시합니다.
        /// </summary>
        private void Update()
        {
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

            if (lastSafeArea != Screen.safeArea || lastScreenSize != screenSize)
            {
                Apply();
            }
        }

        /// <summary>
        /// 현재 Safe Area를 정규화해 상하좌우 베젤 RectTransform에 적용합니다.
        /// </summary>
        private void Apply()
        {
            Rect safe = Screen.safeArea;
            float screenW = Screen.width;
            float screenH = Screen.height;

            if (screenW <= 0f || screenH <= 0f) return;

            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            FitRoot();

            float safeXMin = Mathf.Clamp01(safe.xMin / screenW);
            float safeXMax = Mathf.Clamp01(safe.xMax / screenW);
            float safeYMin = Mathf.Clamp01(safe.yMin / screenH);
            float safeYMax = Mathf.Clamp01(safe.yMax / screenH);

            SetTop(safeYMax);
            SetBottom(safeYMin);
            SetLeft(safeXMin, safeYMin, safeYMax);
            SetRight(safeXMax, safeYMin, safeYMax);
        }

        /// <summary>
        /// 루트 RectTransform을 부모 전체 영역에 맞춥니다.
        /// </summary>
        private void FitRoot()
        {
            RectTransform root = (RectTransform)transform;

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Safe Area 위쪽 바깥 영역을 top 베젤로 채웁니다.
        /// </summary>
        private void SetTop(float safeYMax)
        {
            topBezel.anchorMin = new Vector2(0f, safeYMax);
            topBezel.anchorMax = new Vector2(1f, 1f);
            topBezel.pivot = new Vector2(0.5f, 1f);
            topBezel.anchoredPosition = Vector2.zero;
            topBezel.sizeDelta = Vector2.zero;
            topBezel.gameObject.SetActive(safeYMax < 1f);
        }

        /// <summary>
        /// Safe Area 아래쪽 바깥 영역을 bottom 베젤로 채웁니다.
        /// </summary>
        private void SetBottom(float safeYMin)
        {
            bottomBezel.anchorMin = new Vector2(0f, 0f);
            bottomBezel.anchorMax = new Vector2(1f, safeYMin);
            bottomBezel.pivot = new Vector2(0.5f, 0f);
            bottomBezel.anchoredPosition = Vector2.zero;
            bottomBezel.sizeDelta = Vector2.zero;
            bottomBezel.gameObject.SetActive(safeYMin > 0f);
        }

        /// <summary>
        /// Safe Area 왼쪽 바깥 영역을 left 베젤로 채웁니다.
        /// </summary>
        private void SetLeft(float safeXMin, float safeYMin, float safeYMax)
        {
            leftBezel.anchorMin = new Vector2(0f, safeYMin);
            leftBezel.anchorMax = new Vector2(safeXMin, safeYMax);
            leftBezel.pivot = new Vector2(0f, 0f);
            leftBezel.anchoredPosition = Vector2.zero;
            leftBezel.sizeDelta = Vector2.zero;
            leftBezel.gameObject.SetActive(safeXMin > 0f);
        }

        /// <summary>
        /// Safe Area 오른쪽 바깥 영역을 right 베젤로 채웁니다.
        /// </summary>
        private void SetRight(float safeXMax, float safeYMin, float safeYMax)
        {
            rightBezel.anchorMin = new Vector2(safeXMax, safeYMin);
            rightBezel.anchorMax = new Vector2(1f, safeYMax);
            rightBezel.pivot = new Vector2(1f, 0f);
            rightBezel.anchoredPosition = Vector2.zero;
            rightBezel.sizeDelta = Vector2.zero;
            rightBezel.gameObject.SetActive(safeXMax < 1f);
        }
    }
}
