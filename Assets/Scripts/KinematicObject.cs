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
    
    [Header("Player Interaction")]
    public Transform grabPosition;
    public bool levelObjective;

    [Header("Car Settings")]
    [Tooltip("Optional: An empty GameObject at the front of the car to serve as the origin for the return-path raycast.")]
    public Transform raycastOrigin;
    
    [Header("Pivot Settings (Only for Pivot type)")]
    public Transform pivotAnchor;
    public float maxRotationAngle = 90f;
    
    // Internal state
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _currentRotationAngle;
    private bool _isInteracting;
    private Transform _playerTransform;
    private bool _hasReachedTarget; // Used for Slide/Pivot to lock them
    private bool _isReturning;
    private bool _targetReachedEventFired; // Used to fire the event only once per interaction
    private bool _interactionDisabled = false; // Locks the object after objective completion
    private LevelObjective _levelObjectiveComponent;

    void Awake()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        TryGetComponent(out _levelObjectiveComponent);
        ValidateSetup();
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
        
        Debug.Log($"[KinematicObject] Stopped interaction with {gameObject.name}");
    }
    
    public void AdvanceMovement(float deltaTime)
    {
        if (!_isInteracting) return;
        
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
    
    private void AdvanceSlide(float deltaTime)
    {
        if (targetTransform == null) return;

        if (Vector3.Distance(transform.position, targetTransform.position) < 0.01f)
        {
            HandleTargetReached();
            return;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, speed * deltaTime);
        
        if (Vector3.Distance(transform.position, targetTransform.position) < 0.01f)
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
        
        if (movementType != MovementType.Car)
        {
            _hasReachedTarget = true;
        }
    }
    
    private void AdvancePivot(float deltaTime)
    {
        if (pivotAnchor == null) return;
        
        if (_currentRotationAngle >= maxRotationAngle)
        {
            HandleTargetReached();
            return;
        }
        
        float rotationStep = speed * deltaTime;
        
        if (_currentRotationAngle + rotationStep > maxRotationAngle)
        {
            rotationStep = maxRotationAngle - _currentRotationAngle;
        }
        
        transform.RotateAround(pivotAnchor.position, Vector3.up, rotationStep);
        _currentRotationAngle += rotationStep;
        
        if (_currentRotationAngle >= maxRotationAngle)
        {
            HandleTargetReached();
        }
    }

    private void UpdateCar()
    {
        if (_isReturning && Vector3.Distance(transform.position, _startPosition) > 0.01f)
        {
            Vector3 returnDirection = (_startPosition - transform.position).normalized;
            float returnSpeed = speed * Time.deltaTime;
            float lookAheadDistance = 0.5f;
            
            Vector3 origin = raycastOrigin != null ? raycastOrigin.position : transform.position;

            if (Physics.Raycast(origin, returnDirection, out RaycastHit hit, lookAheadDistance))
            {
                Debug.DrawRay(origin, returnDirection * lookAheadDistance, Color.red);
                
                if (hit.collider.CompareTag("Prop"))
                {
                    if (hit.distance < 0.5f)
                    {
                        _isReturning = false; // Stop the car
                        
                        if (levelObjective && _levelObjectiveComponent != null)
                        {
                            Debug.Log($"[Dev Info] Car stopped by prop '{hit.collider.name}'. Completing level objective.");
                            _levelObjectiveComponent.CompleteObjective();
                            levelObjective = false;
                            _interactionDisabled = true; // Disable future interactions
                        }
                        return; // Exit UpdateCar
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
    
    public bool HasReachedTarget()
    {
        switch (movementType)
        {
            case MovementType.Slide:
            case MovementType.Car:
                if (!targetTransform) return false;
                return Vector3.Distance(transform.position, targetTransform.position) < 0.01f;
                
            case MovementType.Pivot:
                return _currentRotationAngle >= maxRotationAngle;
                
            default:
                return false;
        }
    }
    
    public void ResetToStart()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        _currentRotationAngle = 0f;
        _hasReachedTarget = false;
        _isReturning = false;
        
        if (_isInteracting)
        {
            StopInteraction();
        }
    }
    
    public bool IsInteracting => _isInteracting;
    public bool IsReturning => _isReturning;
    public bool CanInteract => !_interactionDisabled && (movementType == MovementType.Car || !_hasReachedTarget);
    
    void OnDrawGizmos()
    {
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
