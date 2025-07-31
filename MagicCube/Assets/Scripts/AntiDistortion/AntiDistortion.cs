using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiDistortion : PostEffectsBase
{
    public Material material;

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if (material != null) { 
            Graphics.Blit(src, dst, material);
        }
        else {
            Graphics.Blit(src, dst);
        }
    }
}
