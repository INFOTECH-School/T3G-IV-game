using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    public Camera targetCamera;
    void LateUpdate()
    {
        // This makes the object look at the camera every frame
        transform.LookAt(targetCamera.transform);
    }
}