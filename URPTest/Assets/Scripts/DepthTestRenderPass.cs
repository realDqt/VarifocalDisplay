using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthTestRenderPass : ScriptableRenderPass
{
    public Material mMat;
    public int blitShaderPassIndex = 0;
    public FilterMode filterMode { get; set; }
    private RenderTargetIdentifier source { get; set; }
    private RenderTargetHandle destination { get; set; }
    RenderTargetHandle m_temporaryColorTexture;
    string m_ProfilerTag;
    private bool HasReadBack = false;

    public DepthTestRenderPass(string passname, RenderPassEvent _event, Material _mat, float contrast)
    {
        m_ProfilerTag = passname;
        renderPassEvent = _event;
        mMat = _mat;
        m_temporaryColorTexture.Init("temporaryColorTexture");
    }

    public void Setup(RenderTargetIdentifier src, RenderTargetHandle dest)
    {
        source = src;
        destination = dest;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        cmd.GetTemporaryRT(m_temporaryColorTexture.id, desc, FilterMode.Point);
            

        Blit(cmd, source, m_temporaryColorTexture.Identifier(), mMat, blitShaderPassIndex);
        Blit(cmd, m_temporaryColorTexture.Identifier(), destination.Identifier());
        
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(m_temporaryColorTexture.id);
    }
}

