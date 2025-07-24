using UnityEngine;

/// <summary>
/// 挂在 Camera 上的自由飞行脚本。
/// 支持：
///   WASD 水平移动
///   QE   上升/下降
///   鼠标 旋转视角（按住鼠标右键）
///   左Shift 加速
/// </summary>
[RequireComponent(typeof(Camera))]
public class FlyCamera : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2.5f;

    [Header("鼠标旋转")]
    public float mouseSensitivity = 2f;
    public bool lockCursor = true;

    private float pitch;  // 俯仰角
    private float yaw;    // 偏航角

    private void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 初始化旋转
        Vector3 rot = transform.eulerAngles;
        pitch = rot.x;
        yaw = rot.y;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        // 不再检测右键，随时旋转
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        float up = 0f;

        if (Input.GetKey(KeyCode.Q)) up -= 1f;
        if (Input.GetKey(KeyCode.E)) up += 1f;

        Vector3 direction = new Vector3(h, up, v).normalized;

        float speed = moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        // 以相机自身坐标系移动
        transform.Translate(direction * speed, Space.Self);
    }
}