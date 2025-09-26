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

    private int m_Idx = 0;

    public int m_SingleCubeCount = 5;
    public float m_MagicCubeGapTime = 3.0f;
    public float m_SingleCubeGapTime = 1.0f;

    public float[] m_Depths = new float[] {6.0f, 8.0f, 12.36621f, 14.0f, 16.0f };
    private int m_CurCubeCount = 0;
    
    // 拟合的系数
    public Vector4[] m_KR = new Vector4[3] { new Vector4(-4.078462e-04f, -9.498750e-03f, 1.025567e+00f, 1.0f), new Vector4(), new Vector4() };
    public Vector4[] m_KG = new Vector4[3] { new Vector4(-3.828662e-04f, -1.002039e-02f, 1.027372e+00f, 1.0f), new Vector4(), new Vector4() };
    public Vector4[] m_KB = new Vector4[3]{ new Vector4(-3.743558e-04f, -1.014192e-02f, 1.028135e+00f, 1.0f), new Vector4(), new Vector4() };


    private Dictionary<string, bool> m_ResetDic = new Dictionary<string, bool>();

    public GameObject LensController;
    private TunableLensController lensController;

    // Start is called before the first frame update
    void Start()
    {
        lensController = LensController.GetComponent<TunableLensController>();
        SpawnSingleCube();
        
        Debug.Log("Big Cube Center's depth = " + GetDepthFromCamera(new Vector3(0, 0, 0), m_DepthCamera0));
    }

    // Update is called once per frame
    void Update()
    {
        LogDepth();
        CubeMove();
        //SetCoefficient();
    }

    void SetCoefficient()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            m_Idx = 0;
        }else if (Input.GetKeyDown(KeyCode.W))
        {
            m_Idx = 1;
        }else if (Input.GetKeyDown(KeyCode.E))
        {
            m_Idx = 2;
        }
        
        //Debug.Log("Test: coefficient idx = " + m_Idx);

        var antiDistortion = m_DepthCamera0.GetComponent<AntiDistortion>();
        if (antiDistortion)
        {
            antiDistortion.m_KR = m_KR[m_Idx];
            antiDistortion.m_KG = m_KG[m_Idx];
            antiDistortion.m_KB = m_KB[m_Idx]; 
        }
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
                //Debug.Log(key + " Move!");
                if(m_SingleCubes[i] != null) 
                    m_SingleCubes[i].transform.position = Vector3.Lerp(m_SingleCubes[i].transform.position, new Vector3(0, 0, 0), Time.deltaTime);
            }
        }
    }

    
    void SpawnSingleCube()
    {
        if (m_CurCubeCount < m_SingleCubeCount)
        {
            int randIndex = Random.Range(0, 27);
            string path = GetNameByIdx(randIndex);
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + path);
                return;
            }

            var collider = prefab.GetComponent<Collider>() as BoxCollider;

            if (collider == null)
            {
                Debug.LogError("Box Collider not found: " + randIndex);
                return;
            }
            
            float targetDepth = m_Depths[m_CurCubeCount];
            
            
            /*
            Vector3 randPos = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-1f, 1f),
                Random.Range(-5f, 5f)
            );
            */
            Vector3 randPos = GetRandomPositionWithDepth(m_DepthCamera0, targetDepth) - collider.center * 100;
            
            // hack
            Vector3 cubeWorldPos = randPos + collider.center * 100;
           

            // 防止立方体落在大立方体内
            while (cubeWorldPos.x < 1.0f && Mathf.Abs(cubeWorldPos.y) < 1.5f && Mathf.Abs(cubeWorldPos.z) < 1.5f)
            {
                randPos = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-1f, 1f),
                    Random.Range(-5f, 5f)
                );
                cubeWorldPos = randPos + collider.center * 100;
            }
            

            // 记录实例化的对象
            m_SingleCubes[randIndex] = Instantiate(prefab, randPos, Quaternion.identity);
            
            
            Debug.Log("rand pos = " + randPos);
            Debug.Log("collider.center = " +  collider.center);
            Debug.Log("Cube World Position = " + cubeWorldPos);
            
            m_CurCubeCount++;
            

            
            // 计算diopter
            float depth = GetDepthFromCamera(cubeWorldPos, m_DepthCamera0);
            Debug.Log($"Appear: {m_SingleCubes[randIndex].name}  Depth: {depth}");
            float diopter = GetDiopter(depth, m_R, m_Mu, m_ObjectiveLen);
            Debug.Log($"Appear: {m_SingleCubes[randIndex].name}  Diopter: {diopter}");


            Debug.Log($"Object appeared. Distance: {depth:F2}m, Diopter: {diopter:F2}D. Setting static focus.");
            lensController.SetFocalPower(diopter);
        }
    }

    IEnumerator SpawnSingleCubeGap()
    {
        yield return new WaitForSeconds(m_SingleCubeGapTime);
        SpawnSingleCube();
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
                /*
                Collider col = hit.collider;
                Vector3 localCenter = new Vector3(0, 0, 0);
                if (col is BoxCollider box)
                {
                    localCenter = box.center; // 局部坐标
                }
                else if (col is SphereCollider sphere)
                {
                    localCenter = sphere.center;
                }
                else if (col is CapsuleCollider capsule)
                {
                    localCenter = capsule.center;
                }
                else if (col is MeshCollider)
                {
                    Debug.LogError("MeshCollider 没有 .center 属性，无法获取局部中心");
                }
                
                // hack: 该资产cube的position需要修正
                Vector3 cubeWorldPosition = hit.collider.transform.position + localCenter;
                Debug.Log($"Clicked: {hit.collider.name}  World Position: {cubeWorldPosition}");
                
                float depth = GetDepthFromCamera(cubeWorldPosition, m_DepthCamera0);
                Debug.Log($"Clicked: {hit.collider.name}  Depth: {depth}");
                float diopter = GetDiopter(depth, m_R, m_Mu, m_ObjectiveLen);
                Debug.Log($"Clicked: {hit.collider.name}  Diopter: {diopter}");


                Debug.Log($"Object clicked. Distance: {depth:F2}m, Diopter: {diopter:F2}D. Setting static focus.");
                lensController.SetFocalPower(diopter);
                */
                // 归位
                //hit.collider.gameObject.transform.position = Vector3.zero;
                if (!m_ResetDic.ContainsKey(hit.collider.name))
                {
                    m_ResetDic.Add(hit.collider.name, true);
                    Debug.Log("name = " + hit.collider.name);
                    StartCoroutine(SpawnSingleCubeGap());
                    if (m_ResetDic.Count == m_SingleCubeCount)
                    {
                        StartCoroutine(MagicCubeTimeGap());
                    }
                }
                
            }
        }
    }

    IEnumerator MagicCubeTimeGap()
    {
        yield return new WaitForSeconds(m_MagicCubeGapTime);
        MagicCubeTime();
    }

    void MagicCubeTime()
    {
        GameObject.Find("TransparentMagicCube").SetActive(false);
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
    
    Vector3 GetRandomPositionWithDepth(Camera depthCamera, float targetDepth)
    {
        if (depthCamera == null)
        {
            Debug.LogError("depthCamera is null!");
            return Vector3.zero;
        }
    
        // 确保目标深度为正值（在相机前方）
        if (targetDepth <= 0)
        {
            Debug.LogWarning("Target depth should be positive! Using absolute value.");
            targetDepth = Mathf.Abs(targetDepth);
        }
    
        // 计算相机前方目标深度处的基准点
        Vector3 basePosition = depthCamera.transform.position + 
                               depthCamera.transform.forward * targetDepth;
    
        // 获取相机的右方向和上方向（用于生成垂直于前向的平面）
        Vector3 rightDir = depthCamera.transform.right;
        Vector3 upDir = depthCamera.transform.up;
    
        // 在垂直于相机前向的平面上生成随机点
        // 这里使用单位圆内的随机点，可以根据需要调整范围
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(targetDepth * 0.3f, targetDepth * 0.5f);
    
        // 计算随机偏移量
        Vector3 randomOffset = (rightDir * Mathf.Cos(angle) + 
                                upDir * Mathf.Sin(angle)) * distance;
    
        // 返回最终随机位置
        return basePosition + randomOffset;
    }

    float GetDiopter(float d, float R, float mu, float objectiveLen)
    {
        return (R / d - 2 - R / mu) / (-R) - objectiveLen;
    }
}
