using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GetRawDepthRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class HLSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Material mMat;
        public int blitMaterialPassIndex = -1;
        public string textureId = "_ScreenTexture";
        public float contrast = 0.5f;
    }

    public HLSettings settings = new HLSettings();
    RenderTargetHandle m_renderTargetHandle;

    GetRawDepthRenderPass m_ScriptablePass;

    public override void Create()
    {
        int passIndex = settings.mMat != null ? settings.mMat.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        m_ScriptablePass = new GetRawDepthRenderPass("GetRawDepthRenderPass", settings.renderPassEvent, settings.mMat, settings.contrast);
        m_renderTargetHandle.Init(settings.textureId);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = new RenderTargetIdentifier();
        if (settings.mMat == null)
        {
            Debug.LogWarningFormat("missing blit material");
            return;
        }
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


