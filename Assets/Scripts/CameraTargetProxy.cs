using UnityEngine;

public class CameraProxyTarget : MonoBehaviour
{

    // We use LateUpdate to ensure the player has already finished moving/spinning 
    // for this frame before we copy the position.
    void LateUpdate()
    {
        if (GameManager.Instance.Player)
        {
            // Copy position exactly
            transform.position = GameManager.Instance.Player.gameObject.transform.position;
            
            // Notice we do NOT copy transform.rotation! 
            // The proxy stays upright, so the camera stays upright.
        }
    }
}