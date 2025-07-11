using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class Cube : MonoBehaviour
{
    int CubeLayerID;
    public GameObject hitBrick;
    public Camera mainCamera;
    Vector3 mouseClickPos, mouseDragDir;
    Vector3 mouseClickNormal;

    Axis RotateAxis = Axis.None;
    bool isClockwise = false;

    Brick[] bricks;
    private GameObject Rotation;
    List<Brick> rotateBricks = new List<Brick>();
    bool rotateInProcess = false;

    public class RotationData
    {
        public Axis axis;
        public bool isClockwise;
        public int offset = 0;

        public RotationData(Axis axis, bool isClockwise)
        {
            this.axis = axis;
            this.isClockwise = isClockwise;
        }
    }
    Stack<RotationData> rotationStack = new Stack<RotationData>();
    bool recoverInProcess = false;

    // Start is called before the first frame update
    void Start()
    {
        CubeLayerID = LayerMask.NameToLayer("Cube");
        bricks = this.GetComponentsInChildren<Brick>();

        Rotation = new GameObject("Rotation");
        Rotation.transform.position = Vector3.zero;
    }

    private void Rotate(Vector3 cross)
    {
        RotateAxis = Axis.None;

        float x = Vector3.Angle(cross, transform.right);
        float y = Vector3.Angle(cross, transform.up);
        float z = Vector3.Angle(cross, -transform.forward);

        float angle = 0;
        if (x < 15 || x > 165) {
            RotateAxis = Axis.X;
            angle = x;
        } else if (y < 15 || y > 165) {
            RotateAxis = Axis.Y;
            angle = y;
        } else if (z < 15 || z > 165) {
            RotateAxis = Axis.Z;
            angle = z;
        }
        isClockwise = angle < 15;

        Brick child = hitBrick.GetComponent<Brick>();
        if (child != null && RotateAxis != Axis.None)
        {
            RotationData cur_rot = new RotationData(RotateAxis, isClockwise);

            switch (RotateAxis)
            {
                case Axis.X:
                    foreach (var item in bricks)
                    {
                        if (item.offset.x == child.offset.x)
                        {
                            item.gameObject.transform.SetParent(Rotation.transform);
                            rotateBricks.Add(item);
                        }
                    }
                    cur_rot.offset = child.offset.x;
                    break;
                case Axis.Y:
                    foreach (var item in bricks)
                    {
                        if (item.offset.y == child.offset.y)
                        {
                            item.gameObject.transform.SetParent(Rotation.transform);
                            rotateBricks.Add(item);
                        }
                    }
                    cur_rot.offset = child.offset.y;
                    break;
                case Axis.Z:
                    foreach (var item in bricks)
                    {
                        if (item.offset.z == child.offset.z)
                        {
                            item.gameObject.transform.SetParent(Rotation.transform);
                            rotateBricks.Add(item);
                        }
                    }
                    cur_rot.offset = child.offset.z;
                    break;
            }
            rotateInProcess = true;
            rotationStack.Push(cur_rot);
        }
    }

    private void OnMouseDrag()
    {


        // Click on the cube
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, 1 << CubeLayerID))
            {
                hitBrick = hit.transform.gameObject;
                mouseClickPos = hit.point;
                mouseClickNormal = hit.normal;
            }
        }

        // Drag to rotate the cube
        if (Input.GetMouseButton(0) && hitBrick != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, 1 << CubeLayerID))
            {
                mouseDragDir = hit.point - mouseClickPos;
                Vector3 cross = Vector3.Cross(mouseClickNormal, mouseDragDir).normalized;
                if(mouseDragDir.sqrMagnitude > 2f)
                {
                    Rotate(cross);
                    hitBrick = null;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (rotateInProcess)
        {
            float angle = isClockwise ? 90 : -90;
            Vector3 axis = Vector3.zero;
            switch (RotateAxis)
            {
                case Axis.X:
                    axis = transform.right;
                    break;
                case Axis.Y:
                    axis = transform.up;
                    break;
                case Axis.Z:
                    axis = -transform.forward;
                    break;
            }


            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Quaternion rot = Quaternion.Slerp(Rotation.transform.rotation, rotation, 0.08f);
            Rotation.transform.rotation = rot;

            if (Quaternion.Angle(Rotation.transform.rotation, rotation) < 0.02f)
            {
                Rotation.transform.rotation = rotation;

                foreach (var item in rotateBricks)
                {
                    item.UpdateOffset(RotateAxis, isClockwise);
                    item.transform.SetParent(this.transform);
                }
                rotateBricks.Clear();

                Rotation.transform.rotation = Quaternion.identity;
                rotateInProcess = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!rotateInProcess && !recoverInProcess)
        {
            OnMouseDrag();
        }

        if (!rotateInProcess && !recoverInProcess && Input.GetKeyDown(KeyCode.Space))
        {
            Recover();
        }
    }

    public void Recover()
    {
        recoverInProcess = true;
        StartCoroutine(RecoverCube());
    }

    IEnumerator RecoverCube()
    {
        while(rotationStack.Count > 0)
        {
            if (!rotateInProcess)
            {
                RotationData rot = rotationStack.Pop();
                RotateAxis = rot.axis;

                switch (RotateAxis)
                {
                    case Axis.X:
                        foreach (var item in bricks)
                        {
                            if (item.offset.x == rot.offset)
                            {
                                item.gameObject.transform.SetParent(Rotation.transform);
                                rotateBricks.Add(item);
                            }
                        }
                        break;
                    case Axis.Y:
                        foreach (var item in bricks)
                        {
                            if (item.offset.y == rot.offset)
                            {
                                item.gameObject.transform.SetParent(Rotation.transform);
                                rotateBricks.Add(item);
                            }
                        }
                        break;
                    case Axis.Z:
                        foreach (var item in bricks)
                        {
                            if (item.offset.z == rot.offset)
                            {
                                item.gameObject.transform.SetParent(Rotation.transform);
                                rotateBricks.Add(item);
                            }
                        }
                        break;
                }

                isClockwise = !rot.isClockwise;
                rotateInProcess = true;
            }

            yield return null;
        }
        recoverInProcess = false;
    }    
}
