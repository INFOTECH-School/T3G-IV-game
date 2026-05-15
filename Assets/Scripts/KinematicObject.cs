using UnityEngine;
using System;

public class KinematicObject : MonoBehaviour
{
    public event Action OnTargetReached;

    public enum MovementType
    {
        Slide,  // Linear movement between start and target
        Pivot,   // Rotational movement around a pivot point
        Car     // Spring-loaded slide movement
    }

    [Header("Movement Configuration")]
    public MovementType movementType = MovementType.Slide;
    public Transform targetTransform;
    public float speed = 2.0f;
    public bool inverted = false;
    
    [Header("Player Interaction")]
    public Transform grabPosition;
    public bool levelObjective;

    [Header("Car Settings")]
    [Tooltip("Optional: An empty GameObject at the front of the car to serve as the origin for the return-path raycast.")]
    public Transform raycastOrigin;
    
    [Header("Pivot Settings (Only for Pivot type)")]
    public Transform pivotAnchor;
    public float minAngle = 0f;
    public float maxAngle = 90f;

    [Header("Effects")]
    [SerializeField] private GameObject sparkleEffect;
    [SerializeField] private MeshRenderer fall_guide;
    public AudioSource moveAudioSource;

    [Header("Collision Settings (Only for Slide type)")]
    [SerializeField] private LayerMask kinematicObstacleLayer;
    [SerializeField] private Collider _blockingCollider;

    [Header("Gizmo Settings")]
    [SerializeField] private bool _drawDebugGizmos = true;
    [SerializeField] private float _gizmoCastDistance = 0.5f;

    // Internal state
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _isInteracting;
    private Transform _playerTransform;
    private bool _isReturning;
    private bool _targetReachedEventFired;
    public LevelObjective levelObjectiveComponent;
    private bool _isCompleted;
    public bool IsCompleted => _isCompleted;

    private float GetContinuousLocalY()
    {
        float currentLocalY = transform.localEulerAngles.y;
        float midAngle = (minAngle + maxAngle) / 2f;
        return midAngle + Mathf.DeltaAngle(midAngle, currentLocalY);
    }

    void Awake()
    {
        if (minAngle > maxAngle)
        {
            float temp = minAngle;
            minAngle = maxAngle;
            maxAngle = temp;
        }

        _startPosition = transform.position;
        _startRotation = transform.rotation;

        if (levelObjectiveComponent == null)
        {
            TryGetComponent(out levelObjectiveComponent);
        }
        ValidateSetup();
        if (fall_guide)
        {
            fall_guide.enabled = false;
        }

        if (_blockingCollider == null)
        {
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (!col.isTrigger)
                {
                    _blockingCollider = col;
                    break;
                }
            }
        }

        if (_blockingCollider == null)
        {
            Debug.LogWarning($"[KinematicObject] {gameObject.name}: No non-trigger Collider found or assigned. Collision checks for Slide movement will not work.", this);
        }
        
        if (sparkleEffect)
        {
            sparkleEffect.SetActive(true);
        }

        // Pre-load audio data to RAM to prevent stuttering on first interaction
        if (moveAudioSource != null && moveAudioSource.clip != null)
        {
            moveAudioSource.clip.LoadAudioData();
        }
    }

    void Start()
    {
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            float currentContinuousY = GetContinuousLocalY();
            Vector3 upAxis = transform.parent != null ? transform.parent.up : Vector3.up;

            if (currentContinuousY < minAngle - 0.05f || currentContinuousY > maxAngle + 0.05f)
            {
                float midAngle = (minAngle + maxAngle) / 2f;
                float angleDiff = midAngle - currentContinuousY;
                
                transform.RotateAround(pivotAnchor.position, upAxis, angleDiff);
                
                _startPosition = transform.position;
                _startRotation = transform.rotation;
            }
        }
    }

    void Update()
    {
        if (movementType == MovementType.Car)
        {
            UpdateCar();
        }
    }
    
    void ValidateSetup()
    {
        if (targetTransform == null) Debug.LogError($"[KinematicObject] {gameObject.name}: targetTransform is not assigned!", this);
        if (grabPosition == null) Debug.LogError($"[KinematicObject] {gameObject.name}: grabPosition is not assigned!", this);
        if (movementType == MovementType.Pivot && pivotAnchor == null) Debug.LogError($"[KinematicObject] {gameObject.name}: Pivot type requires pivotAnchor!", this);
        if (transform.localScale != Vector3.one) Debug.LogWarning($"[KinematicObject] {gameObject.name}: Non-uniform scale detected!", this);
    }
    
    public void StartInteraction(Transform player)
    {
        if (_isInteracting || !CanInteract) return;
        
        _isInteracting = true;
        _isReturning = false;
        _targetReachedEventFired = false; 
        _playerTransform = player;
        
        player.position = grabPosition.position;
        player.rotation = grabPosition.rotation;
        player.SetParent(transform, true);

        if (sparkleEffect)
        {
            sparkleEffect.SetActive(false);
        }

        if (fall_guide)
        {
            fall_guide.enabled = true;
        }
        
        Debug.Log($"[KinematicObject] Started interaction with {gameObject.name}");
    }
    
    public void StopInteraction()
    {
        if (!_isInteracting) return;
        
        _isInteracting = false;
        
        if (_playerTransform != null)
        {
            _playerTransform.SetParent(null);
            _playerTransform = null;
        }

        if (movementType == MovementType.Car)
        {
            _isReturning = true;
        }

        if (sparkleEffect)
        {
            sparkleEffect.SetActive(!_isCompleted);
        }

        if (fall_guide)
        {
            fall_guide.enabled = false;
        }
        
        StopSound();
        Debug.Log($"[KinematicObject] Stopped interaction with {gameObject.name}");
    }
    
    public void AdvanceMovement(float deltaTime)
    {
        if (!_isInteracting) return;
        
        if (inverted)
        {
            switch (movementType)
            {
                case MovementType.Slide:
                case MovementType.Car:
                    ReverseSlide(deltaTime);
                    break;
                case MovementType.Pivot:
                    ReversePivot(deltaTime);
                    break;
            }
        }
        else
        {
            switch (movementType)
            {
                case MovementType.Slide:
                case MovementType.Car:
                    AdvanceSlide(deltaTime);
                    break;
                case MovementType.Pivot:
                    AdvancePivot(deltaTime);
                    break;
            }
        }
    }
    
    private void AdvanceSlide(float deltaTime)
    {
        if (targetTransform == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = targetTransform.position;
        Vector3 moveDirection = (targetPosition - currentPosition).normalized;
        float maxMoveDistance = speed * deltaTime;
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        float actualMoveDistance = Mathf.Min(maxMoveDistance, distanceToTarget);

        if (actualMoveDistance < 0.01f)
        {
            HandleTargetReached();
            return;
        }

        if (_blockingCollider != null && _blockingCollider is BoxCollider)
        {
            BoxCollider boxCollider = _blockingCollider as BoxCollider;
            Vector3 worldCenter = transform.TransformPoint(boxCollider.center);
            Vector3 scaledHalfExtents = Vector3.Scale(boxCollider.size / 2, transform.lossyScale);
            Quaternion castRotation = transform.rotation;
            float skinWidth = 0.01f;
            float checkDistance = actualMoveDistance + skinWidth;

            RaycastHit hit;
            if (Physics.BoxCast(worldCenter, scaledHalfExtents, moveDirection, out hit, castRotation, checkDistance, kinematicObstacleLayer))
            {
                if (hit.collider.gameObject != this.gameObject && hit.collider.GetComponent<KinematicObject>() != null)
                {
                    Debug.Log($"[KinematicObject] {gameObject.name} Slide movement blocked by another KinematicObject: {hit.collider.name}", this);
                    return;
                }
            }
        }

        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, actualMoveDistance);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            HandleTargetReached();
        }
    }

    private void HandleTargetReached()
    {
        if (!_targetReachedEventFired)
        {
            _targetReachedEventFired = true;
            OnTargetReached?.Invoke();
            StopSound();
        }
    }
    
    private void AdvancePivot(float deltaTime)
    {
        if (!pivotAnchor) return;

        float currentY = GetContinuousLocalY();
        float rotationAmount = speed * deltaTime;
        float newAngle = currentY + rotationAmount;
        
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        float rotationStep = newAngle - currentY;

        if (Mathf.Abs(rotationStep) > 0.001f)
        {
            Vector3 upAxis = transform.parent != null ? transform.parent.up : Vector3.up;
            transform.RotateAround(pivotAnchor.position, upAxis, rotationStep);
        }

        if (Mathf.Abs(newAngle - maxAngle) < 0.01f)
        {
            HandleTargetReached();
        }
    }

    public void PlaySound()
    {
        if (!moveAudioSource) return;
        if (!moveAudioSource.isPlaying)
        {
            moveAudioSource.Play();
        }
    }

    public void StopSound()
    {
        if (!moveAudioSource) return;
        if (moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
        }
    }
    

    public void ReverseMovement(float deltaTime)
    {
        if (!_isInteracting) return;

        if (inverted)
        {
            switch (movementType)
            {
                case MovementType.Slide:
                case MovementType.Car:
                    AdvanceSlide(deltaTime);
                    break;
                case MovementType.Pivot:
                    AdvancePivot(deltaTime);
                    break;
            }
        }
        else
        {
            switch (movementType)
            {
                case MovementType.Slide:
                case MovementType.Car:
                    ReverseSlide(deltaTime);
                    break;
                case MovementType.Pivot:
                    ReversePivot(deltaTime);
                    break;
            }
        }
    }

    private void ReverseSlide(float deltaTime)
    {
        if (Vector3.Distance(transform.position, _startPosition) < 0.01f) return;

        Vector3 currentPosition = transform.position;
        Vector3 moveDirection = (_startPosition - currentPosition).normalized;
        float maxMoveDistance = speed * deltaTime;
        float distanceToStart = Vector3.Distance(currentPosition, _startPosition);
        float actualMoveDistance = Mathf.Min(maxMoveDistance, distanceToStart);

        if (_blockingCollider != null && _blockingCollider is BoxCollider)
        {
            BoxCollider boxCollider = _blockingCollider as BoxCollider;
            Vector3 worldCenter = transform.TransformPoint(boxCollider.center);
            Vector3 scaledHalfExtents = Vector3.Scale(boxCollider.size / 2, transform.lossyScale);
            Quaternion castRotation = transform.rotation;
            float skinWidth = 0.01f;
            float checkDistance = actualMoveDistance + skinWidth;

            RaycastHit hit;
            if (Physics.BoxCast(worldCenter, scaledHalfExtents, moveDirection, out hit, castRotation, checkDistance, kinematicObstacleLayer))
            {
                if (hit.collider.gameObject != this.gameObject && hit.collider.GetComponent<KinematicObject>() != null)
                {
                    Debug.Log($"[KinematicObject] {gameObject.name} Reverse slide blocked by another KinematicObject: {hit.collider.name}", this);
                    return;
                }
            }
        }

        transform.position = Vector3.MoveTowards(currentPosition, _startPosition, actualMoveDistance);
    }

    private void ReversePivot(float deltaTime)
    {
        if (!pivotAnchor) return;

        float currentY = GetContinuousLocalY();
        float rotationAmount = speed * deltaTime;
        float newAngle = currentY - rotationAmount;

        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        float rotationStep = newAngle - currentY;

        if (Mathf.Abs(rotationStep) > 0.001f)
        {
            Vector3 upAxis = transform.parent != null ? transform.parent.up : Vector3.up;
            transform.RotateAround(pivotAnchor.position, upAxis, rotationStep);
        }
    }

    private void UpdateCar()
    {
        if (_isReturning && Vector3.Distance(transform.position, _startPosition) > 0.01f)
        {
            Vector3 returnDirection = (_startPosition - transform.position).normalized;
            float returnSpeed = speed * Time.deltaTime;
            float lookAheadDistance = 0.5f;
            
            Vector3 origin = raycastOrigin ? raycastOrigin.position : transform.position;

            if (Physics.Raycast(origin, returnDirection, out RaycastHit hit, lookAheadDistance))
            {
                Debug.DrawRay(origin, returnDirection * lookAheadDistance, Color.red);
                
                if (hit.collider.CompareTag("Prop"))
                {
                    if (hit.distance < 0.5f)
                    {
                        _isReturning = false;
                        if (levelObjective && levelObjectiveComponent != null)
                        {
                            levelObjectiveComponent.CompleteObjective();
                            targetTransform.gameObject.SetActive(false);
                            if (sparkleEffect)
                            {
                                sparkleEffect.SetActive(false);
                            }
                            levelObjective = false;
                            _isCompleted = true;
                        }
                        return;
                    }
                }
                else if (hit.collider.CompareTag("Player") && hit.distance < 1.0f)
                {
                    hit.transform.position += returnDirection * returnSpeed;
                }
            }
            else
            {
                Debug.DrawRay(origin, returnDirection * lookAheadDistance, Color.green);
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _startPosition, returnSpeed);

            if (Vector3.Distance(transform.position, _startPosition) < 0.01f)
            {
                _isReturning = false;
            }
        }
    }

    public void CompleteObjective()
    {
        if (movementType == MovementType.Car && levelObjectiveComponent != null)
        {
            levelObjectiveComponent.CompleteObjective();
        }

        sparkleEffect.SetActive(false);
        _isCompleted = true;
    }
    
    public void DisableInteraction()
    {
        _isCompleted = true;
        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }
    }
    
    public bool HasReachedTarget()
    {
        switch (movementType)
        {
            case MovementType.Slide:
            case MovementType.Car:
                if (!targetTransform) return false;
                return Vector3.Distance(transform.position, targetTransform.position) < 0.01f;
                
            case MovementType.Pivot:
                return Mathf.Abs(GetContinuousLocalY() - maxAngle) < 0.01f;
                
            default:
                return false;
        }
    }
    
    public void ResetToStart()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _isReturning = false;
        
        if (_isInteracting)
        {
            StopInteraction();
        }
    }
    
    public bool IsInteracting => _isInteracting;
    public bool IsReturning => _isReturning;
    public bool CanInteract => !_isCompleted;
    
    private void OnDrawGizmos()
    {
        // --- Original Gizmos ---
        if (grabPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(grabPosition.position, 0.2f);
            Gizmos.DrawLine(grabPosition.position, grabPosition.position + grabPosition.forward * 0.5f);
        }
        
        if (targetTransform != null)
        {
            if (movementType == MovementType.Slide || movementType == MovementType.Car)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(targetTransform.position, Vector3.one * 0.5f);
                
                Gizmos.color = Color.yellow;
                Vector3 startPoss = Application.isPlaying ? _startPosition : transform.position;
                Gizmos.DrawLine(startPoss, targetTransform.position);
            }
        }
        
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pivotAnchor.position, 0.3f);
            Vector3 upAxis = transform.parent != null ? transform.parent.up : Vector3.up;
            Gizmos.DrawLine(pivotAnchor.position, pivotAnchor.position + upAxis * 1f);
        }

        // --- New BoxCast Gizmo ---
        if (!_drawDebugGizmos || movementType != MovementType.Slide || !(_blockingCollider is BoxCollider) || targetTransform == null)
        {
            return;
        }

        BoxCollider boxCollider = _blockingCollider as BoxCollider;
        Vector3 worldCenter = transform.TransformPoint(boxCollider.center);
        Vector3 scaledHalfExtents = Vector3.Scale(boxCollider.size / 2, transform.lossyScale);
        Quaternion castRotation = transform.rotation;
        Vector3 forwardDirection = (targetTransform.position - transform.position).normalized;
        Vector3 startPos = Application.isPlaying ? _startPosition : transform.position;
        Vector3 reverseDirection = (startPos - transform.position).normalized;

        Matrix4x4 originalMatrix = Gizmos.matrix;

        // Origin box
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, castRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scaledHalfExtents * 2);

        // Forward cast end
        Gizmos.color = Color.red;
        Vector3 forwardEnd = worldCenter + forwardDirection * _gizmoCastDistance;
        Gizmos.matrix = Matrix4x4.TRS(forwardEnd, castRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scaledHalfExtents * 2);

        // Reverse cast end
        Gizmos.color = Color.magenta;
        Vector3 reverseEnd = worldCenter + reverseDirection * _gizmoCastDistance;
        Gizmos.matrix = Matrix4x4.TRS(reverseEnd, castRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scaledHalfExtents * 2);

        Gizmos.matrix = originalMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldCenter, forwardEnd);
        Gizmos.DrawLine(worldCenter, reverseEnd);
    }

    void OnDrawGizmosSelected()
    {
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.yellow;
            
            float midAngle = (minAngle + maxAngle) / 2f;
            float continuousCurrentY = midAngle + Mathf.DeltaAngle(midAngle, transform.localEulerAngles.y);

            Vector3 currentDirection = transform.position - pivotAnchor.position;
            Vector3 upAxis = transform.parent != null ? transform.parent.up : Vector3.up;
            Vector3 fromDirection = Quaternion.AngleAxis(-continuousCurrentY, upAxis) * currentDirection;

            int segments = 20;
            float totalAngle = maxAngle - minAngle;
            float angleStep = totalAngle / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = minAngle + angleStep * i;
                float angle2 = minAngle + angleStep * (i + 1);

                Vector3 point1 = pivotAnchor.position + Quaternion.AngleAxis(angle1, upAxis) * fromDirection;
                Vector3 point2 = pivotAnchor.position + Quaternion.AngleAxis(angle2, upAxis) * fromDirection;

                Gizmos.DrawLine(point1, point2);
            }
        }
    }
}
