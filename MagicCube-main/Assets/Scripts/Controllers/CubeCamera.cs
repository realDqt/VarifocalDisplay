using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCamera : MonoBehaviour
{
    public GameObject Cube;

    public float speed_x, speed_y;
    private float distance;
    private float euler_x, euler_y;
    
    // Start is called before the first frame update
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        euler_x = angles.y;
        euler_y = angles.x;

        distance = Vector3.Distance(transform.position, Cube.transform.position);
    }
    
    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float delta_x = Input.GetAxis("Mouse X") * speed_x * Time.deltaTime;
            float delta_y = Input.GetAxis("Mouse Y") * speed_y * Time.deltaTime;

            UpdateCamera(delta_x,delta_y);
        }
    }
    
    public void UpdateCamera(float delta_x,float delta_y)
    {
        euler_x += delta_x;
        euler_y = Mathf.Clamp(euler_y - delta_y, -60, 60);

        Quaternion rot = Quaternion.Euler(euler_y, euler_x, 0);
        transform.rotation = rot;
        
        Vector3 pos = rot * new Vector3(0, 0, -distance) + Cube.transform.position;
        transform.position = pos;
    }
}
