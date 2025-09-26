using UnityEngine;

/// <summary>
/// 在鼠标位置画一个小圆圈，编辑器与运行模式均可见。
/// 挂在场景里任意空物体上即可。
/// </summary>
[AddComponentMenu("Utils/MousePointer")]
public class MousePointer : MonoBehaviour
{
    // 圆圈的半径
    public float circleRadius = 10f;
    // 圆圈的颜色
    public Color circleColor = Color.white;
    // 圆圈的线宽
    public float lineWidth = 2f;

    void OnGUI()
    {
        // 获取鼠标在屏幕上的位置
        Vector2 mousePosition = Event.current.mousePosition;
        // 转换坐标（因为GUI的原点在左上角，而屏幕坐标通常以左下角为原点）
        //mousePosition.y = Screen.height - mousePosition.y;

        // 开始绘制
        GL.PushMatrix();
        // 创建一个临时材质
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = circleColor;
        lineMaterial.SetPass(0);

        // 设置线宽
        GL.Begin(GL.LINES);
        GL.Color(circleColor);

        // 绘制圆圈（由多个线段组成）
        int segments = 36; // 线段数量，越多圆圈越平滑
        for (int i = 0; i < segments; i++)
        {
            // 计算当前点的角度
            float angle = (float)i / segments * Mathf.PI * 2;
            // 计算下一个点的角度
            float nextAngle = (float)(i + 1) / segments * Mathf.PI * 2;

            // 计算当前点的位置
            Vector2 currentPoint = new Vector2(
                Mathf.Cos(angle) * circleRadius + mousePosition.x,
                Mathf.Sin(angle) * circleRadius + mousePosition.y
            );

            // 计算下一个点的位置
            Vector2 nextPoint = new Vector2(
                Mathf.Cos(nextAngle) * circleRadius + mousePosition.x,
                Mathf.Sin(nextAngle) * circleRadius + mousePosition.y
            );

            // 添加线段
            GL.Vertex(currentPoint);
            GL.Vertex(nextPoint);
        }

        GL.End();
        GL.PopMatrix();

        // 释放材质资源
        Destroy(lineMaterial);
    }
}