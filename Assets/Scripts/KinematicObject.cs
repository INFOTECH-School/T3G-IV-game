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
    public float maxRotationAngle = 90f;

    [Header("Effects")]
    [SerializeField] private GameObject sparkleEffect;
    [SerializeField] private MeshRenderer fall_guide;

    [Header("Collision Settings (Only for Slide type)")]
    [SerializeField] private LayerMask kinematicObstacleLayer;
    [SerializeField] private Collider _blockingCollider;

    [Header("Gizmo Settings")]
    [SerializeField] private bool _drawDebugGizmos = true;
    [SerializeField] private float _gizmoCastDistance = 0.5f;

    // Internal state
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _currentRotationAngle;
    private bool _isInteracting;
    private Transform _playerTransform;
    private bool _isReturning;
    private bool _targetReachedEventFired;
    private LevelObjective _levelObjectiveComponent;
    private bool _isCompleted;

    void Awake()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        TryGetComponent(out _levelObjectiveComponent);
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
        }
    }
    
    private void AdvancePivot(float deltaTime)
    {
        if (!pivotAnchor) return;

        float rotationDirection = Mathf.Sign(maxRotationAngle);
        float targetAngle = maxRotationAngle;

        if ((rotationDirection > 0 && _currentRotationAngle >= targetAngle) ||
            (rotationDirection < 0 && _currentRotationAngle <= targetAngle))
        {
            HandleTargetReached();
            return;
        }

        float rotationStep = speed * deltaTime * rotationDirection;

        if ((rotationDirection > 0 && _currentRotationAngle + rotationStep > targetAngle) ||
            (rotationDirection < 0 && _currentRotationAngle + rotationStep < targetAngle))
        {
            rotationStep = targetAngle - _currentRotationAngle;
        }

        transform.RotateAround(pivotAnchor.position, Vector3.up, rotationStep);
        _currentRotationAngle += rotationStep;

        if ((rotationDirection > 0 && _currentRotationAngle >= targetAngle) ||
            (rotationDirection < 0 && _currentRotationAngle <= targetAngle))
        {
            HandleTargetReached();
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

        transform.position = Vector3.MoveTowards(transform.position, _startPosition, speed * deltaTime);
    }

    private void ReversePivot(float deltaTime)
    {
        if (!pivotAnchor) return;

        float rotationDirection = Mathf.Sign(maxRotationAngle);

        if ((rotationDirection > 0 && _currentRotationAngle <= 0) ||
            (rotationDirection < 0 && _currentRotationAngle >= 0))
        {
            return;
        }

        float rotationStep = speed * deltaTime * rotationDirection;

        if ((rotationDirection > 0 && _currentRotationAngle - rotationStep < 0) ||
            (rotationDirection < 0 && _currentRotationAngle - rotationStep > 0))
        {
            rotationStep = _currentRotationAngle;
        }

        transform.RotateAround(pivotAnchor.position, Vector3.down, rotationStep);
        _currentRotationAngle -= rotationStep;
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
                        if (levelObjective && _levelObjectiveComponent != null)
                        {
                            _levelObjectiveComponent.CompleteObjective();
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
        // if (_levelObjectiveComponent)
        // {
        //     _levelObjectiveComponent.CompleteObjective();
        // }

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
                if (maxRotationAngle > 0)
                    return _currentRotationAngle >= maxRotationAngle;
                else
                    return _currentRotationAngle <= maxRotationAngle;
                
            default:
                return false;
        }
    }
    
    public void ResetToStart()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _currentRotationAngle = 0f;
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
                Vector3 startPos = Application.isPlaying ? _startPosition : transform.position;
                Gizmos.DrawLine(startPos, targetTransform.position);
            }
        }
        
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pivotAnchor.position, 0.3f);
            Gizmos.DrawLine(pivotAnchor.position, pivotAnchor.position + Vector3.up * 1f);
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
        Vector3 moveDirection = (targetTransform.position - transform.position).normalized;

        Matrix4x4 originalMatrix = Gizmos.matrix;

        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, castRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scaledHalfExtents * 2);

        Gizmos.color = Color.red;
        Vector3 endCenter = worldCenter + moveDirection * _gizmoCastDistance;
        Gizmos.matrix = Matrix4x4.TRS(endCenter, castRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scaledHalfExtents * 2);

        Gizmos.matrix = originalMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldCenter, endCenter);
    }

    void OnDrawGizmosSelected()
    {
        if (movementType == MovementType.Pivot && pivotAnchor != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 from = transform.position - pivotAnchor.position;
            
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
