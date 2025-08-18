using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Transparent : MonoBehaviour
{
    public float m_Alpha = 0.3f;

    private Material material;
    private Color originalColor;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("找不到 Renderer！");
            return;
        }

        material = renderer.material;

        // 保存原始颜色
        originalColor = material.color;

        // 强制把 Standard Shader 切换为 Transparent 模式
        SetMaterialTransparent(material);
    }

    void Update()
    {
        float alpha = m_Alpha;

        Color newColor = originalColor;
        newColor.a = alpha;
        material.color = newColor;

        if (alpha <= 0f)
            enabled = false; // 停止更新
    }

    // 把 Standard Shader 切换为 Transparent 渲染模式
    private void SetMaterialTransparent(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Mode", 2); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}