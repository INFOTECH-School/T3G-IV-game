using UnityEngine;

/// <summary>
/// Kinematic puzzle object that moves along a fixed path (no physics).
/// Supports both sliding (linear) and pivoting (rotational) movement.
/// </summary>
public class KinematicObject : MonoBehaviour
{
    public enum MovementType
    {
        Slide,  // Linear movement between start and target
        Pivot   // Rotational movement around a pivot point
    }

    [Header("Movement Configuration")]
    [Tooltip("Type of movement for this object")]
    public MovementType movementType = MovementType.Slide;
    
    [Tooltip("Transform that defines the end position/rotation")]
    public Transform targetTransform;
    
    [Tooltip("Movement speed (units/sec for Slide, degrees/sec for Pivot)")]
    public float speed = 2.0f;
    
    [Header("Player Interaction")]
    [Tooltip("Position where the player will be placed when interacting")]
    public Transform grabPosition;
    
    [Header("Pivot Settings (Only for Pivot type)")]
    [Tooltip("The anchor point to rotate around (e.g., door hinge)")]
    public Transform pivotAnchor;
    
    [Tooltip("Maximum rotation angle in degrees (for clamping)")]
    public float maxRotationAngle = 90f;
    
    // Internal state
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _currentRotationAngle;
    private bool _isInteracting;
    private Transform _playerTransform;
    
    void Awake()
    {
        // Cache starting position and rotation
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        
        // Validation
        ValidateSetup();
    }
    
    void ValidateSetup()
    {
        if (targetTransform == null)
        {
            Debug.LogError($"[KinematicObject] {gameObject.name}: targetTransform is not assigned!", this);
        }
        
        if (grabPosition == null)
        {
            Debug.LogError($"[KinematicObject] {gameObject.name}: grabPosition is not assigned!", this);
        }
        
        if (movementType == MovementType.Pivot && pivotAnchor == null)
        {
            Debug.LogError($"[KinematicObject] {gameObject.name}: Pivot type requires pivotAnchor!", this);
        }
        
        // Scale warning
        if (transform.localScale != Vector3.one)
        {
            Debug.LogWarning($"[KinematicObject] {gameObject.name}: Non-uniform scale detected! " +
                           "This may cause player deformation when parented. Use scale on child mesh instead.", this);
        }
    }
    
    /// <summary>
    /// Called by PlayerInteraction when player presses X to start interacting
    /// </summary>
    public void StartInteraction(Transform player)
    {
        if (_isInteracting) return;
        
        _isInteracting = true;
        _playerTransform = player;
        
        // Snap player to grab position
        player.position = grabPosition.position;
        player.rotation = grabPosition.rotation;
        
        // Parent player to this object (keep world position)
        player.SetParent(transform, true);
        
        Debug.Log($"[KinematicObject] Started interaction with {gameObject.name}");
    }
    
    /// <summary>
    /// Called by PlayerInteraction when player releases X or reaches target
    /// </summary>
    public void StopInteraction()
    {
        if (!_isInteracting) return;
        
        _isInteracting = false;
        
        // Un-parent player
        if (_playerTransform != null)
        {
            _playerTransform.SetParent(null);
            _playerTransform = null;
        }
        
        Debug.Log($"[KinematicObject] Stopped interaction with {gameObject.name}");
    }
    
    /// <summary>
    /// Called by PlayerMovement when player holds W during interaction
    /// </summary>
    public void AdvanceMovement(float deltaTime)
    {
        if (!_isInteracting) return;
        
        switch (movementType)
        {
            case MovementType.Slide:
                AdvanceSlide(deltaTime);
                break;
            case MovementType.Pivot:
                AdvancePivot(deltaTime);
                break;
        }
    }
    
    /// <summary>
    /// Linear movement towards target position
    /// </summary>
    private void AdvanceSlide(float deltaTime)
    {
        if (targetTransform == null) return;
        
        // Check if already at target
        if (Vector3.Distance(transform.position, targetTransform.position) < 0.01f)
        {
            return;
        }
        
        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetTransform.position,
            speed * deltaTime
        );
    }
    
    /// <summary>
    /// Rotational movement around pivot anchor
    /// </summary>
    private void AdvancePivot(float deltaTime)
    {
        if (pivotAnchor == null) return;
        
        // Check if reached max rotation
        if (_currentRotationAngle >= maxRotationAngle)
        {
            return;
        }
        
        // Calculate rotation step
        float rotationStep = speed * deltaTime;
        
        // Clamp to max angle
        if (_currentRotationAngle + rotationStep > maxRotationAngle)
        {
            rotationStep = maxRotationAngle - _currentRotationAngle;
        }
        
        // Rotate around the pivot anchor (World Up axis)
        transform.RotateAround(
            pivotAnchor.position,
            Vector3.up,
            rotationStep
        );
        
        _currentRotationAngle += rotationStep;
    }
    
    /// <summary>
    /// Check if object has reached its target
    /// </summary>
    public bool HasReachedTarget()
    {
        switch (movementType)
        {
            case MovementType.Slide:
                if (targetTransform == null) return false;
                return Vector3.Distance(transform.position, targetTransform.position) < 0.01f;
                
            case MovementType.Pivot:
                return _currentRotationAngle >= maxRotationAngle;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Reset object to starting position/rotation (for level reset)
    /// </summary>
    public void ResetToStart()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _currentRotationAngle = 0f;
        
        if (_isInteracting)
        {
            StopInteraction();
        }
    }
    
    public bool IsInteracting => _isInteracting;
    
    // Gizmos for editor visualization
    void OnDrawGizmos()
    {
        // Draw grab position
        if (grabPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(grabPosition.position, 0.2f);
            Gizmos.DrawLine(grabPosition.position, grabPosition.position + grabPosition.forward * 0.5f);
        }
        
        // Draw target position/rotation indicator
        if (targetTransform != null)
        {
            if (movementType == MovementType.Slide)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(targetTransform.position, Vector3.one * 0.5f);
                
                // Draw path line
            Gizmos.color = Color.yellow;
            Vector3 startPos = Application.isPlaying ? _startPosition : transform.position;
            Gizmos.DrawLine(startPos, targetTransform.position);
            }
        }
        
        // Draw pivot anchor
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pivotAnchor.position, 0.3f);
            Gizmos.DrawLine(pivotAnchor.position, pivotAnchor.position + Vector3.up * 1f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw rotation arc for pivot objects
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 from = transform.position - pivotAnchor.position;
            
            // Draw arc segments
            int segments = 20;
            float angleStep = maxRotationAngle / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = angleStep * i;
                float angle2 = angleStep * (i + 1);
                
                Vector3 point1 = pivotAnchor.position + Quaternion.Euler(0, angle1, 0) * from;
                Vector3 point2 = pivotAnchor.position + Quaternion.Euler(0, angle2, 0) * from;
                
                Gizmos.DrawLine(point1, point2);
            }
        }
    }
}

