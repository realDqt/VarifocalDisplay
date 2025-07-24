using System.Collections;
using System.Collections.Generic;
using Leap;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public float orbitalSpeed = 200;

    public GameObject Cube;

    private LeapProvider leapProvider = null;
    public Camera mainCamera;

    public CubeCamera CubeCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (leapProvider == null)
        {
            leapProvider = Hands.Provider;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var hand in leapProvider.CurrentFrame.Hands)
        {
            if (hand.GrabStrength == 0)
            {
                // Debug.LogWarning(hand.PalmNormal);

                Vector3 projectedVector = Vector3.ProjectOnPlane(hand.PalmNormal, Vector3.up);
                
                Debug.LogWarning(projectedVector);

                CubeCamera.UpdateCamera(projectedVector.x,projectedVector.z);

            }
        }
    }
}
