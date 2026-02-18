using UnityEngine;

/// <summary>
/// Simple test/debug script for KinematicObject
/// Attach to a KinematicObject to see debug info in play mode
/// </summary>
[RequireComponent(typeof(KinematicObject))]
public class KinematicObjectDebug : MonoBehaviour
{
    private KinematicObject _kinematicObject;
    
    [Header("Debug Display")]
    public bool showDebugInfo = true;
    public Color gizmoColor = Color.magenta;
    
    void Start()
    {
        _kinematicObject = GetComponent<KinematicObject>();
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || _kinematicObject == null) return;
        
        // Display status in top-left corner
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Box($"Kinematic Object: {gameObject.name}");
        GUILayout.Label($"Type: {_kinematicObject.movementType}");
        GUILayout.Label($"Interacting: {_kinematicObject.IsInteracting}");
        GUILayout.Label($"Reached Target: {_kinematicObject.HasReachedTarget()}");
        
        if (_kinematicObject.movementType == KinematicObject.MovementType.Slide && _kinematicObject.targetTransform != null)
        {
            float distance = Vector3.Distance(transform.position, _kinematicObject.targetTransform.position);
            GUILayout.Label($"Distance to Target: {distance:F2}");
        }
        
        GUILayout.EndArea();
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Draw a wire sphere around the object for visibility
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

