using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
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
            topLayout = GameObject.FindGameObjectWithTag(UnityConstant.Tags.GameplayTopLayout).GetComponent<RectTransform>();
            centerLayout = GameObject.FindGameObjectWithTag(UnityConstant.Tags.GameplayCenterLayout).GetComponent<RectTransform>();
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
        /// 지정한 UI를 Off
        /// </summary>
        public void Close()
        {

        }

        /// <summary>
        /// 지정한 UI를 획득/인스턴싱
        /// </summary>
        //public T Get<T>()
        //{
        //    return null;
        //}
    }
}
