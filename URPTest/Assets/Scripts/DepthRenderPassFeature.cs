using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//必须得用到这个namespace？
//namespace UnityEngine.Experiemntal.Rendering.Universal
//{
    public class DepthRenderPassFeature : ScriptableRendererFeature
    {
        public enum Target
        {
            Color,
            Texture
        }
        [System.Serializable]
        public class HLSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            public Material mMat;
            public Target destination = Target.Color;
            public int blitMaterialPassIndex = -1;
            public string textureId = "_ScreenTexture";
            public float contrast = 0.5f;
        }

        public HLSettings settings = new HLSettings();
        RenderTargetHandle m_renderTargetHandle;

        DepthRenderPass m_ScriptablePass;

        public override void Create()
        {
            int passIndex = settings.mMat != null ? settings.mMat.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            m_ScriptablePass = new DepthRenderPass("DepthRenderPass", settings.renderPassEvent, settings.mMat, settings.contrast);
            m_renderTargetHandle.Init(settings.textureId);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = new RenderTargetIdentifier();
            var dest = (settings.destination == Target.Color) ? RenderTargetHandle.CameraTarget : m_renderTargetHandle;
            if (settings.mMat == null)
            {
                Debug.LogWarningFormat("missing blit material");
                return;
            }
            m_ScriptablePass.Setup(src,dest);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
//}


