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

        private static Canvas worldCanvas;

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
            worldCanvas = GameObjectEx.FindByLayer<Canvas>(UnityConstant.Layers.WorldUIIndex);
            cameraCanvas = GameObjectEx.FindByLayer<Canvas>(UnityConstant.Layers.CameraUIIndex);

            Cursor.SetCursor(settings.CursorShape.texture, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// 월드 좌표를 화면 좌표로 변환합니다.
        /// </summary>
        public Vector3 WorldPosToUIPos(Vector3 worldPosition, RectTransform targetRect = null, Camera worldCamera = null)
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
        public void Close(int instanceId)
        {
            uiInsts.Remove(instanceId);
        }

        /// <summary>
        /// 지정한 UI 타입의 인스턴스를 가져오거나 생성합니다.
        /// </summary>
        public T Open<T>(RenderSpace renderSpace) where T : UIBase
        {
            T pb = settings.Get<T>();

            if (pb == null)
            {
                return null;
            }

            Transform root = null;
            switch (renderSpace)
            {
                case RenderSpace.Overlay:
                    root = overlayCanvas.transform;
                    break;

                case RenderSpace.World:
                    root = worldCanvas.transform;
                    break;

                case RenderSpace.Camera:
                default:
                    root = cameraCanvas.transform;
                    break;
            }

            T inst = Instantiate(pb, root, false);
            uiInsts.Add(inst.GetInstanceID(), inst);

            return inst;
        }
    }
}
