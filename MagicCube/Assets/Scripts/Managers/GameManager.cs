using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("depthCamera0")] public Camera m_DepthCamera0;

    private GameObject[] m_SingleCubes = new GameObject[27];

    private GameObject m_MagicCube;

    private float m_R = 0.05f;
    private float m_Mu = 0.039f;
    private float m_ObjectiveLen = 55.6f;


    private Dictionary<string, bool> m_ResetDic = new Dictionary<string, bool>();
    // Start is called before the first frame update
    void Start()
    {
        SpawnSingleCubes();
    }

    // Update is called once per frame
    void Update()
    {
        LogDepth();
        CubeMove();
    }

    string GetNameByIdx(int i)
    {
        bool hasZero = i < 10;
        return "SingleCube" + (hasZero ? 0 : "") + i;
    }

    void CubeMove()
    {
        for (int i = 0; i < 27; i++)
        {
            string key = GetNameByIdx(i) + "(Clone)";
            if (m_ResetDic.ContainsKey(key))
            {
                Debug.Log(key + " Move!");
                if(m_SingleCubes[i] != null) 
                    m_SingleCubes[i].transform.position = Vector3.Lerp(m_SingleCubes[i].transform.position, new Vector3(0, 0, 0), Time.deltaTime);
            }
        }
    }

    void SpawnSingleCubes()
    {
        for (int i = 0; i < 27; i++)
        {
            string path = GetNameByIdx(i);
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + path);
                continue;
            }

            Vector3 randPos = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-1f, 1f),
                Random.Range(-10f, 10f)
            );

            // 记录实例化的对象
            m_SingleCubes[i] = Instantiate(prefab, randPos, Quaternion.identity);
        }
    }

    void LogDepth()
    {
        // 只在鼠标左键按下时检测
        if (Input.GetMouseButtonDown(0))
        {
            // 从鼠标位置发射一条射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 如果射线击中了带有 Collider 的物体
            if (Physics.Raycast(ray, out hit))
            {
                // 打印被点击物体的名字和世界坐标
                
                Collider col = hit.collider;
                Vector3 localCenter = new Vector3(0, 0, 0);
                if (col is BoxCollider box)
                {
                    localCenter = box.center; // 局部坐标
                    //Debug.Log("BoxCollider 局部中心: " + localCenter);
                }
                else if (col is SphereCollider sphere)
                {
                    localCenter = sphere.center;
                    //Debug.Log("SphereCollider 局部中心: " + localCenter);
                }
                else if (col is CapsuleCollider capsule)
                {
                    localCenter = capsule.center;
                    //Debug.Log("CapsuleCollider 局部中心: " + localCenter);
                }
                else if (col is MeshCollider)
                {
                    Debug.Log("MeshCollider 没有 .center 属性，无法获取局部中心");
                }
                // hack: 该资产cube的position需要修正
                Vector3 cubeWorldPosition = hit.collider.transform.position + localCenter;
                Debug.Log($"Clicked: {hit.collider.name}  World Position: {hit.collider.transform.position + localCenter}");
                
                float depth = GetDepthFromCamera(cubeWorldPosition, m_DepthCamera0);
                Debug.Log($"Clicked: {hit.collider.name}  Depth: {depth}");
                Debug.Log($"Clicked: {hit.collider.name}  Diopter: {GetDiopter(depth, m_R, m_Mu, m_ObjectiveLen)}");
                // 归位
                //hit.collider.gameObject.transform.position = Vector3.zero;
                if (!m_ResetDic.ContainsKey(hit.collider.name))
                {
                    m_ResetDic.Add(hit.collider.name, true);
                    Debug.Log("name = " + hit.collider.name);
                    if (m_ResetDic.Count == 27)
                    {
                        MagicCubeTime();
                    }
                }
                
            }
        }
    }

    void MagicCubeTime()
    {
        GameObject magicCubePrefab = Resources.Load<GameObject>("RubikCube");
        if (magicCubePrefab == null)
        {
            Debug.LogError("RubikCube prefab not found in Resources folder!");
            return;
        }

        m_MagicCube= Instantiate(magicCubePrefab, Vector3.zero, Quaternion.identity);
        m_MagicCube.GetComponent<Cube>().mainCamera = m_DepthCamera0;

        // 销毁实例化的对象
        for (int i = 0; i < 27; i++)
        {
            if (m_SingleCubes[i] != null)
            {
                Destroy(m_SingleCubes[i]);
            }
        }
    }
    
    float GetDepthFromCamera(Vector3 worldPosition, Camera depthCamera)
    {
        if (depthCamera == null)
        {
            Debug.Log("depthCamera is null!");
            return float.MaxValue;
        }
        Vector3 deltaVec =  worldPosition - depthCamera.transform.position;
        Vector3 viewDir = Vector3.Normalize(depthCamera.transform.forward);
        return Vector3.Dot(deltaVec, viewDir);
    }

    float GetDiopter(float d, float R, float mu, float objectiveLen)
    {
        float k = R * R / mu;
        return (R / d - 2.0f) / k - objectiveLen;
    }
}
