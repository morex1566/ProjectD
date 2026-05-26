using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TRPG.Runtime
{
    /// <summary>
    /// 에디터 SceneView를 제외하고 런타임 카메라에만 Fullscreen Pass를 적용합니다.
    /// </summary>
    public class RuntimeOnlyFullScreenPassRendererFeature : FullScreenPassRendererFeature
    {
        [SerializeField] private bool includeSceneView;

        /// <summary>
        /// SceneView 카메라에는 CRT 패스를 등록하지 않습니다.
        /// </summary>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!includeSceneView && renderingData.cameraData.isSceneViewCamera) return;

            base.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
