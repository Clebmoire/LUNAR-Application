using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
