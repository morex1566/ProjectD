using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    /// <summary>
    /// 게임플레이 UI 루트와 공통 UI 프리팹 생성을 관리합니다.
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        /// <summary>
        /// UIManager가 생성하거나 제어하는 UI 종류입니다.
        /// </summary>
        public enum UIType
        {
            Damage
        }

        private static UIManagerSettingsData settings;

        public UnityEvent<float, float> OnResolutionChanged = new();

        private static Canvas Gameplay;

        private static RectTransform topLayout;

        private static RectTransform centerLayout;

        /// <summary>
        /// UI 매니저 초기화 진입점입니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<UIManagerSettingsData>("SO_UIManagerSettings");
            Gameplay = GameObject.FindGameObjectWithTag(UnityConstant.Tags.GameplayCanvas).GetComponent<Canvas>();
            ConfigureCanvasesForCrt(Gameplay.transform.root, Camera.main);
            topLayout = GameObject.FindGameObjectWithTag(UnityConstant.Tags.GameplayTopLayout).GetComponent<RectTransform>();
            centerLayout = GameObject.FindGameObjectWithTag(UnityConstant.Tags.GameplayCenterLayout).GetComponent<RectTransform>();
        }

        /// <summary>
        /// Overlay Canvas는 카메라 후처리 뒤에 그려지므로 CRT가 적용되도록 카메라 렌더 경로로 옮깁니다.
        /// </summary>
        private static void ConfigureCanvasesForCrt(Transform canvasRoot, Camera renderCamera)
        {
            if (renderCamera == null)
            {
                Debug.LogWarning("[UIManager] CRT 적용을 위한 MainCamera를 찾지 못했습니다.");
                return;
            }

            Canvas[] canvases = canvasRoot.GetComponentsInChildren<Canvas>(true);

            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = renderCamera;
                canvas.planeDistance = 100f;
            }
        }

        /// <summary>
        /// 월드 좌표를 화면 좌표로 변환합니다.
        /// </summary>
        public Vector3 WorldPosToGameplayUIPos(Vector3 worldPosition, RectTransform targetRect = null, Camera worldCamera = null)
        {
            worldCamera ??= Camera.main;
            targetRect ??= Gameplay.transform as RectTransform;

            // 월드 좌표를 화면 좌표로 변환합니다.
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(worldCamera,worldPosition);

            // Canvas Render Mode에 따라 UI 카메라를 결정합니다.
            Camera uiCamera = Gameplay.renderMode == RenderMode.ScreenSpaceOverlay ? null : Gameplay.worldCamera;

            // 화면 좌표를 대상 RectTransform 로컬 좌표로 변환합니다.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPosition, uiCamera, out Vector2 anchoredPos);

            return anchoredPos;
        }

        public void ShowDamage(Vector3 worldPosition, float damage)
        {
            GameObject damageUI = Instantiate(settings.DamageUIPb, centerLayout, false);

            if (damageUI.transform is not RectTransform rectTransform) return;

            rectTransform.anchoredPosition = WorldPosToGameplayUIPos(worldPosition, centerLayout);
        }

        /// <summary>
        /// 현재 열려 있는 UI를 닫습니다.
        /// </summary>
        public void Close()
        {

        }

        /// <summary>
        /// 지정한 UI 타입의 인스턴스를 가져오거나 생성합니다.
        /// </summary>
        //public T Get<T>()
        //{
        //    return null;
        //}
    }
}
