using System.Collections.Generic;
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
        public enum RenderSpace
        {
            Overlay,
            World,
            Camera
        }

        private static UIManagerSettingsData settings;

        private static Canvas overlayCanvas;

        private static Canvas cameraCanvas;

        /// <summary>
        /// UIBase.GetInstanceID()를 키로 현재 모든 UI를 관리
        /// </summary>
        private static readonly Dictionary<int, UIBase> uiInsts = new();

        public UnityEvent<float, float> OnResolutionChanged = new();

        /// <summary>
        /// UI 매니저 초기화 진입점입니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<UIManagerSettingsData>("SO_UIManagerSettings");

            overlayCanvas = GameObjectEx.FindByLayer<Canvas>(UnityConstant.Layers.OverlayUIIndex);
            cameraCanvas = GameObjectEx.FindByLayer<Canvas>(UnityConstant.Layers.CameraUIIndex);

            Cursor.SetCursor(settings.CursorShape.texture, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// 월드 좌표를 화면 좌표로 변환합니다.
        /// </summary>
        public static Vector3 WorldPosToUIPos(Vector3 worldPosition, RectTransform targetRect = null, Camera worldCamera = null)
        {
            return GetInstance().WorldPosToUIPosInternal(worldPosition, targetRect, worldCamera);
        }

        /// <summary>
        /// 현재 열려 있는 UI를 닫습니다.
        /// </summary>
        public static void Close(int instanceId)
        {
            GetInstance().CloseInternal(instanceId);
        }

        /// <summary>
        /// 지정한 UI 타입의 인스턴스를 가져오거나 생성합니다.
        /// </summary>
        public static T Open<T>(RenderSpace renderSpace) where T : UIBase
        {
            return GetInstance().OpenInternal<T>(renderSpace);
        }



        /// <summary>
        /// 현재 Canvas 설정을 기준으로 월드 좌표를 UI 로컬 좌표로 변환합니다.
        /// </summary>
        private Vector3 WorldPosToUIPosInternal(Vector3 worldPosition, RectTransform targetRect = null, Camera worldCamera = null)
        {
            worldCamera ??= Camera.main;
            targetRect ??= overlayCanvas.transform as RectTransform;

            // 월드 좌표를 화면 좌표로 변환합니다.
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);

            // Canvas Render Mode에 따라 UI 카메라를 결정합니다.
            Camera uiCamera = overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : overlayCanvas.worldCamera;

            // 화면 좌표를 대상 rectTransform 로컬 좌표로 변환합니다.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPosition, uiCamera, out Vector2 anchoredPos);

            return anchoredPos;
        }

        /// <summary>
        /// 현재 열려 있는 UI를 닫습니다.
        /// </summary>
        private void CloseInternal(int instanceId)
        {
            uiInsts.Remove(instanceId);
        }

        /// <summary>
        /// 지정한 UI 타입의 인스턴스를 가져오거나 생성합니다.
        /// </summary>
        private T OpenInternal<T>(RenderSpace renderSpace) where T : UIBase
        {
            T pb = settings.Get<T>();

            if (pb == null)
            {
                return null;
            }

            T inst = null;
            switch (renderSpace)
            {
                case RenderSpace.Overlay:
                    inst = Instantiate(pb, overlayCanvas.transform, false);
                    break;

                case RenderSpace.Camera:
                    inst = Instantiate(pb, cameraCanvas.transform, false);
                    break;

                case RenderSpace.World:
                    inst = Instantiate(pb);
                    break;

                default:
                    break;
            }

            uiInsts.Add(inst.GetInstanceID(), inst);

            return inst;
        }
    }
}
