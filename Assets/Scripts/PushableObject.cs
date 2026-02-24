using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public bool isPushable = true;
    
    [Tooltip("Miejsce, w którym pojawią się ręce gracza")]
    public Transform grabPoint; 

    private void OnDrawGizmos()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(grabPoint.position, 0.2f);
        }
    }
}