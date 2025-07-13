using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using Unity.Collections;
using UnityEngine.Experimental.Rendering; // ← for File
                 
public class GetRawDepthRenderPass : ScriptableRenderPass
{
    public Material mMat;
    public int blitShaderPassIndex = 0;
    public NativeArray<float> depthValues;
    public FilterMode filterMode { get; set; }
    private RenderTargetIdentifier source { get; set; }
    private RenderTargetHandle destination { get; set; }
    RenderTargetHandle m_temporaryColorTexture;
    RenderTexture m_rt; // bug
    string m_ProfilerTag;
    private bool HasReadBack = false;

    public GetRawDepthRenderPass(string passname, RenderPassEvent _event, Material _mat, float contrast)
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
            
        // 1) 构造 R32_SFloat 单通道 32-bit 浮点 RT
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.graphicsFormat = GraphicsFormat.R32_SFloat;
        RenderTexture rt = RenderTexture.GetTemporary(desc);
            
        // 2) Blit 到浮点 RT
        Blit(cmd, source, rt, mMat, blitShaderPassIndex);
            
        // 3) 只第一次发起异步读回
        if (!HasReadBack)
        {
            cmd.RequestAsyncReadback(
                rt,
                0,
                (AsyncGPUReadbackRequest req) =>
                {
                    if (req.hasError)
                    {
                        Debug.LogError("AsyncGPUReadback error");
                        return;
                    }
                    // 拿到 width*height 个 float
                    var data = req.GetData<float>();
                    depthValues.ResizeArray(data.Length);
                    depthValues.CopyFrom(data);
                    Debug.Log("depthValues.Length = " + depthValues.Length);
                    foreach (var depthValue in depthValues)
                    {
                        //Debug.Log(depthValue);
                    }
                }
            );
            HasReadBack = true;
        }
        
            
        // 4) 执行 & 释放
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        RenderTexture.ReleaseTemporary(rt);
    
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        RenderTexture.ReleaseTemporary(m_rt);
    }
}
