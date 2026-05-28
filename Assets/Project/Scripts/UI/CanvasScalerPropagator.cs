using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 기준 Canvas의 CanvasScaler 설정을 하위 Canvas들에 복사합니다.
    /// </summary>
    public class CanvasScalerPropagator : MonoBehaviour
    {
        [Header("Reference Canvas")]

        [SerializeField] private Canvas referenceCanvas;

        /// <summary>
        /// 에디터에서 값이 변경될 때 자식 CanvasScaler 설정을 동기화합니다.
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Sync();
            }
#endif
        }

        /// <summary>
        /// 기준 CanvasScaler 설정을 모든 하위 Canvas에 복사합니다.
        /// </summary>
        [ContextMenu("Sync Canvas Scaler Settings")]
        public void Sync()
        {
            if (referenceCanvas == null)
            {
                Debug.LogWarning("[CanvasScalerPropagator] Reference Canvas가 없습니다.");
                return;
            }

            CanvasScaler sourceScaler = referenceCanvas.GetComponent<CanvasScaler>();
            if (sourceScaler == null)
            {
                Debug.LogWarning("[CanvasScalerPropagator] Reference Canvas에 CanvasScaler가 없습니다.");
                return;
            }

            Canvas[] canvases = GetComponentsInChildren<Canvas>(true);

            foreach (Canvas targetCanvas in canvases)
            {
                if (targetCanvas == referenceCanvas)
                    continue;

                SyncCanvasScaler(sourceScaler, targetCanvas);
            }
        }

        /// <summary>
        /// 단일 대상 Canvas의 CanvasScaler를 기준 설정과 일치시킵니다.
        /// </summary>
        private void SyncCanvasScaler(CanvasScaler sourceScaler, Canvas targetCanvas)
        {
            CanvasScaler targetScaler = targetCanvas.GetComponent<CanvasScaler>();

            if (targetScaler == null)
                targetScaler = targetCanvas.gameObject.AddComponent<CanvasScaler>();

            targetScaler.uiScaleMode = sourceScaler.uiScaleMode;
            targetScaler.referenceResolution = sourceScaler.referenceResolution;
            targetScaler.screenMatchMode = sourceScaler.screenMatchMode;
            targetScaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
            targetScaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;

            targetScaler.scaleFactor = sourceScaler.scaleFactor;
            targetScaler.physicalUnit = sourceScaler.physicalUnit;
            targetScaler.fallbackScreenDPI = sourceScaler.fallbackScreenDPI;
            targetScaler.defaultSpriteDPI = sourceScaler.defaultSpriteDPI;
            targetScaler.dynamicPixelsPerUnit = sourceScaler.dynamicPixelsPerUnit;

#if UNITY_EDITOR
            EditorUtility.SetDirty(targetScaler);
#endif
        }
    }
}
